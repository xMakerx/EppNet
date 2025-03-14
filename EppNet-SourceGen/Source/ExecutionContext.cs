/////////////////////////////////////////////
/// Filename: ExecutionContext.cs
/// Date: March 12, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace EppNet.SourceGen
{
    public class ExecutionContext
    {

        /// <summary>
        /// Whether or not this is running in the analysis mode
        /// </summary>
        public bool IsAnalysis;

        public IDictionary<string, string> Resolvers;

        private ConcurrentDictionary<string, string> _stringPool;

        // Can store TypeSyntax or strings
        private ConcurrentDictionary<object, (bool, bool)> _typesCache;
        private ConcurrentDictionary<ITypeSymbol, string> _typeNames;

        public ExecutionContext(bool isAnalysis)
        {
            this.IsAnalysis = isAnalysis;
            this.Resolvers = null;
            this._stringPool = new();
            this._typesCache = new();
            this._typeNames = new(SymbolEqualityComparer.Default);
        }

        public bool CacheType(TypeSyntax typeSyntax, string typeName, bool isValid = true, bool isNetObject = false) =>
            _typesCache.TryAdd(typeSyntax, (isValid, isNetObject)) &&
            _typesCache.TryAdd(typeName, (isValid, isNetObject));

        public (bool, bool) CacheTypeAndReturn(TypeSyntax typeSyntax, string typeName, bool isValid = true, bool isNetObject = false)
        {
            _typesCache.TryAdd(typeSyntax, (isValid, isNetObject));
            _typesCache.TryAdd(typeName, (isValid, isNetObject));
            return (isValid, isNetObject);
        }

        public (bool, (bool, bool)) GetType(string typeName)
        {
            bool found = _typesCache.TryGetValue(typeName, out (bool, bool) results);
            return (found, results);
        }

        public (bool, (bool, bool)) GetType(TypeSyntax typeSyntax)
        {
            bool found = _typesCache.TryGetValue(typeSyntax, out (bool, bool) results);
            return (found, results);
        }

        public string GetTypeName(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return string.Empty;

            if (_typeNames.TryGetValue(typeSymbol, out var typeName))
                return typeName;

            typeName = typeSymbol.GetFullyQualifiedName();
            _typeNames.TryAdd(typeSymbol, typeName);

            return GetString(typeName);
        }

        public string GetString(string input) => 
            _stringPool.GetOrAdd(input, input);

    }

}

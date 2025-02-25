/////////////////////////////////////////////
/// Filename: Globals.cs
/// Date: February 8, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using EppNet.SourceGen.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EppNet.SourceGen
{

    /// <summary>
    /// Defines global symbols using during source generation
    /// </summary>
    public static class Globals
    {
        public const string DataPath = "EppNet.Data.";
        public const string AttrPath = "EppNet.Attributes.";

        public const string NetworkObjectInterfaceName = "INetworkObject";
        public const string NetworkObjectInternalInterfaceName = "INetworkObject_Impl";

        public const string TypeResolverName = "Resolver";
        public const string TypeResolverGenericName = TypeResolverName + "<T>";
        public const string TypeResolverFullName = DataPath + TypeResolverName;
        public const string TypeResolverFullGenericName = DataPath + TypeResolverGenericName;

        public const string Attribute = "Attribute";

        //////////////////////////////////////////////
        /// Attribute for locating resolvers
        //////////////////////////////////////////////
        public const string NetTypeResolverAttr = "NetworkTypeResolver";
        public const string NetTypeResolverAttrFullName = AttrPath + NetTypeResolverAttr + Attribute;
        //////////////////////////////////////////////

        //////////////////////////////////////////////
        /// Attribute for locating network object type definitions
        //////////////////////////////////////////////
        public const string NetObjectAttr = "NetworkObject";
        public const string NetObjectAttrFullName = AttrPath + NetObjectAttr + Attribute;

        public const string NetMethodAttr = "NetworkMethod";
        public const string NetMethodAttrFullName = AttrPath + NetMethodAttr + Attribute;

        public static readonly string[] SupportedTypes =
        [
            "System.Collections.Generic.List",
            "System.Collections.Generic.HashSet",
            "System.Collections.Generic.SortedSet",
            "System.Collections.Generic.Dictionary",
            "System.Collections.Generic.LinkedList"
        ];

        //////////////////////////////////////////////
        /// Descriptors
        ////////////////////////////////////////////// 
        
        /// <summary>
        /// Thrown when there is some kind of issue with a network type resolver
        /// </summary>
        public static DiagnosticDescriptor DescDebug = new(
            id: "EPN001",
            title: "Analyzer Debug",
            messageFormat: "E++Net: {0}",
            category: "SourceGenerator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor DescError = new(
            id: "EPN002",
            title: "Analyzer Error",
            messageFormat: "E++Net: {0}",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// Thrown when there is some kind of issue with a network type resolver
        /// </summary>
        public static DiagnosticDescriptor DescTypeResolverError = new(
            id: "EPN003",
            title: "Network Type Resolver Error",
            messageFormat: "E++Net: {0}",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// Thrown when there is some kind of error with a network object
        /// </summary>
        public static DiagnosticDescriptor DescNetObjError = new(
            id: "EPN004",
            title: "Network Object Error",
            messageFormat: "E++Net: {0}",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor DescNetMethodError = new(
            id: "EPN005",
            title: "Network Method Error",
            messageFormat: "E++Net: {0}",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static bool HasAttribute(ClassDeclarationSyntax classNode, string attrName)
        {
            foreach (var attrList in classNode.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (attr.Name.ToString() == attrName)
                        return true;
                }
            }

            return false;
        }

        public static bool HasAttribute(MethodDeclarationSyntax methodNode, string attrName)
        {
            foreach (var attrList in methodNode.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (attr.Name.ToString() == attrName)
                        return true;
                }
            }

            return false;
        }
        

        // Checks if the specified type name is valid
        // (IsValid, IsNetObject)
        public static (bool, bool) IsValidTypeName(string typeName, IDictionary<string, string> resolverDict,
            IDictionary<string, NetworkObjectModel?> objDict)
        {

            if (SupportedTypes.Contains(typeName))
                return (true, false);

            if (resolverDict.ContainsKey(typeName))
                return (true, false);

            if (objDict.ContainsKey(typeName))
                return (true, true);

            return (false, false);
        }

        public static (NetworkParameterTypeModel?, TypeSyntax) ExamineType(TypeSyntax type, SemanticModel semModel,
            IDictionary<string, string> resolverDict,
            IDictionary<string, NetworkObjectModel?> objDict, CancellationToken cancelToken = default)
        {
            // We want to:
            // 1) Examine the type to see if it's supported.
            // - 1a: If it's a generic name, ensure the base type name is valid.
            // -     Enums, Dictionary, List, HashSet, SortedSet, and LinkedList are valid
            // 2) Check if the type has a resolver.
            NetworkParameterTypeModel? model = null;
            ITypeSymbol typeSymbol = semModel.GetTypeInfo(type, cancelToken).Type;

            cancelToken.ThrowIfCancellationRequested();

            if (typeSymbol == null)
                return (model, null);

            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                // This is an enum type. These always have an integral underlying type
                ITypeSymbol underlyingType = ((INamedTypeSymbol)typeSymbol).EnumUnderlyingType;
                string underlyingTypeName = underlyingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                model = new NetworkParameterTypeModel(typeSymbol, null, underlyingType: underlyingTypeName);
                return (model, null);
            }

            if (type is ArrayTypeSyntax arrayType)
                return ExamineType(arrayType.ElementType, semModel, resolverDict, objDict, cancelToken);

            if (type is GenericNameSyntax genericName)
            {
                string baseTypeName = $"{typeSymbol.ContainingNamespace.ToDisplayString()}.{genericName.Identifier.Text}";
                (bool isValidType, bool isNetObj) = IsValidTypeName(baseTypeName, resolverDict, objDict);

                if (!isValidType)
                    return (model, type);

                EquatableList<NetworkParameterTypeModel> subtypes = new();
                foreach (TypeSyntax typeArg in genericName.TypeArgumentList.Arguments)
                {
                    var result = ExamineType(typeArg, semModel, resolverDict, objDict, cancelToken);

                    // Let's ensure the result is valid
                    if (result.Item1 == null)
                        return (model, result.Item2);

                    subtypes.Add(result.Item1.Value);
                }

                model = new NetworkParameterTypeModel(typeSymbol, subtypes, null, isNetObject: isNetObj);
            }
            else if (type is TupleTypeSyntax tuple)
            {
                EquatableList<NetworkParameterTypeModel> subtypes = new();
                foreach (TupleElementSyntax tupleArg in tuple.Elements)
                {
                    TypeSyntax typeArg = tupleArg.Type;
                    var result = ExamineType(typeArg, semModel, resolverDict, objDict, cancelToken);

                    // Let's ensure the result is valid
                    if (result.Item1 == null)
                        return (model, result.Item2);

                    subtypes.Add(result.Item1.Value);
                }

                model = new NetworkParameterTypeModel(typeSymbol, subtypes, null, isNetObject: false);
            }
            else
            {
                string fullTypeName = $"{typeSymbol.ContainingNamespace.ToDisplayString()}.{typeSymbol.Name}";
                (bool isValidType, bool isNetObj) = IsValidTypeName(fullTypeName, resolverDict, objDict);

                if (!isValidType)
                    return (model, type);

                model = new NetworkParameterTypeModel(typeSymbol, null, isNetObject: isNetObj);
            }

            return (model, null);
        }

        public static (NetworkMethodModel?, NetworkMethodAnalysisError, ParameterSyntax, TypeSyntax) LocateNetMethod(SyntaxNode node, SemanticModel semModel, 
            IDictionary<string, string> resolverDict,
            IDictionary<string, NetworkObjectModel?> objDict, CancellationToken cancelToken = default)
        {

            // We want to:
            // 1) Ensure the method is accessible (public)
            // 2) Ensure the method is part of a class decorated with a NetworkObject attribute
            // 3) Ensure the parameter types have a network resolver

            NetworkMethodModel? model = null;
            NetworkMethodAnalysisError error = NetworkMethodAnalysisError.None;

            cancelToken.ThrowIfCancellationRequested();

            if (cancelToken.IsCancellationRequested || node is not MethodDeclarationSyntax methodNode)
                return (model, NetworkMethodAnalysisError.NotMethod, null, null);

            IMethodSymbol symbol = semModel.GetDeclaredSymbol(methodNode);

            if (symbol == null)
                return (model, NetworkMethodAnalysisError.NotMethod, null, null);

            // Step 1: Ensure the method is accessible
            if (!methodNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                error = NetworkMethodAnalysisError.Inaccessible;

            // Step 2: Ensure the method is part of a class decorated with a NetworkObject attribute
            ClassDeclarationSyntax classNode = methodNode.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();

            if (classNode != null && !HasAttribute(classNode, NetObjectAttr))
                error |= NetworkMethodAnalysisError.NotNetworkObjectClass;

            // Step 3: Ensure the parameters types have a network resolver
            List<string> typeNames = new();

            EquatableList<NetworkParameterTypeModel> parameters = new();

            foreach (ParameterSyntax paramNode in methodNode.ParameterList.Parameters)
            {
                TypeSyntax type = paramNode.Type;

                (NetworkParameterTypeModel? typeModel, TypeSyntax typeArg) = ExamineType(type, semModel, resolverDict, objDict, cancelToken);

                if (!typeModel.HasValue)
                {
                    // Type is invalid.
                    return (model, error | NetworkMethodAnalysisError.MissingResolver, paramNode, typeArg);
                }

                parameters.Add(typeModel.Value);
            }

            model = new NetworkMethodModel(symbol, parameters);

            INamedTypeSymbol classSymbol = semModel.GetDeclaredSymbol(classNode);
            string className = $"{classSymbol.ContainingNamespace.ToDisplayString()}.{classSymbol.Name}";

            if (!objDict.TryGetValue(className, out NetworkObjectModel? netModel))
                error |= NetworkMethodAnalysisError.NotNetworkObjectClass;
            else
            {
                NetworkObjectModel objModel = netModel.Value;
                bool added = false;

                for (int i = 0; i < objModel.Methods.Count; i++)
                {
                    NetworkMethodModel methodModel = objModel.Methods[i];

                    if (methodModel.Equals(model))
                    {
                        objModel.Methods[i] = model.Value;
                        added = true;
                        break;
                    }
                }

                if (!added)
                    objModel.Methods.Add(model.Value);
            }

            return (model, error, null, null);
        }

        public static (NetworkObjectModel?, NetworkObjectAnalysisError) LocateNetObject(SyntaxNode node, SemanticModel semModel,
            CancellationToken cancelToken = default)
        {
            // We want to:
            // 1) Ensure the class inherits from INetworkObject
            // 2) Ensure the class is partial
            NetworkObjectModel? model = null;
            NetworkObjectAnalysisError error = NetworkObjectAnalysisError.None;

            cancelToken.ThrowIfCancellationRequested();

            if (cancelToken.IsCancellationRequested || node is not ClassDeclarationSyntax classNode)
                return (model, NetworkObjectAnalysisError.NotClass);

            INamedTypeSymbol symbol = semModel.GetDeclaredSymbol(classNode);

            if (symbol == null)
                return (model, error);

            // Step 1: Ensure class is partial
            if (!classNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                error |= NetworkObjectAnalysisError.NotPartial;

            // Step 2: Ensure class inherits from INetworkObject
            if (!symbol.AllInterfaces.Any(s => s.Name == NetworkObjectInterfaceName))
                error |= NetworkObjectAnalysisError.LacksInheritance;

            if (error == NetworkObjectAnalysisError.None)
                model = new(symbol, []);

            return (model, error);
        }
        
        public static (ResolverModel?, ResolverAnalysisError) LocateResolver(SyntaxNode node, SemanticModel semModel, CancellationToken cancelToken = default)
        {
            // We want to:
            // 1) Ensure the class inherits from Resolver<T>
            // 2) Ensure the class has a public singleton symbol defined at the top
            ResolverModel? model = null;
            ResolverAnalysisError error = ResolverAnalysisError.None;

            cancelToken.ThrowIfCancellationRequested();

            if (cancelToken.IsCancellationRequested || node is not ClassDeclarationSyntax classNode)
                return (model, ResolverAnalysisError.NotClass);

            INamedTypeSymbol symbol = semModel.GetDeclaredSymbol(classNode);

            if (symbol == null)
                return (model, error);

            // Step 1: Ensure class has singleton field/property named Instance
            ISymbol singletonSymbol = symbol.GetMembers("Instance").FirstOrDefault(m =>
                m.IsStatic &&
                m.DeclaredAccessibility == Accessibility.Public &&
                ((m is IFieldSymbol field && field.Type.Name == symbol.Name) ||
                (m is IPropertySymbol prop && prop.Type.Name == symbol.Name))
            );

            // If we didn't locate it, return an error'd out ResolverModel
            if (singletonSymbol == null)
                error = ResolverAnalysisError.LacksSingleton;

            // Step 2: Ensure class inherits from Resolver<T>
            INamedTypeSymbol baseTypeSymbol = symbol.BaseType;

            while (baseTypeSymbol != null)
            {
                if (baseTypeSymbol.OriginalDefinition.ToDisplayString() == TypeResolverFullGenericName)
                {
                    // Let's extract the type argument
                    if (baseTypeSymbol.IsGenericType && baseTypeSymbol.TypeArguments.Length == 1)
                    {
                        // Great! We located it!
                        ITypeSymbol genericType = baseTypeSymbol.TypeArguments[0];
                        model = new ResolverModel(symbol, genericType);
                        break;
                    }
                }

                baseTypeSymbol = baseTypeSymbol.BaseType;
            }

            if (model == null)
            {
                // We didn't locate the base type.
                error |= ResolverAnalysisError.LacksInheritance;
            }

            return (model, error);
        }
      
        public static bool IsMatchingType(ITypeSymbol a, INamedTypeSymbol b) =>
            SymbolEqualityComparer.IncludeNullability.Equals(a, b) ||
            SymbolEqualityComparer.Default.Equals(a, b) ||
            a.ToDisplayString() == b.ToDisplayString();
    }



}
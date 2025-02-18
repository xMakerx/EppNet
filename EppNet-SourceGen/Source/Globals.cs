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

            foreach (ParameterSyntax paramNode in methodNode.ParameterList.Parameters)
            {
                TypeSyntax type = paramNode.Type;
                ITypeSymbol typeSymbol = semModel.GetTypeInfo(type).Type;

                if (typeSymbol == null)
                    continue;

                if (type is GenericNameSyntax genericName)
                {
                    // Fully qualified base type name
                    string baseTypeName = $"{typeSymbol.ContainingNamespace.ToDisplayString()}.{genericName.Identifier.Text}";
                    //typeNames.Add(baseTypeName);

                    // Get generic argument type names
                    foreach (var typeArg in genericName.TypeArgumentList.Arguments)
                    {
                        var typeArgSymbol = semModel.GetTypeInfo(typeArg).Type;
                        if (typeArgSymbol != null)
                        {
                            string argTypeName = $"{typeArgSymbol.ContainingNamespace.ToDisplayString()}.{typeArgSymbol.Name}";
                            typeNames.Add(argTypeName); 
                        }
                        else
                        {
                            typeNames.Add(typeArg.ToString()); // Fallback to raw syntax name
                        }
                    }
                }
                else
                {
                    string fullTypeName = $"{typeSymbol.ContainingNamespace.ToDisplayString()}.{typeSymbol.Name}";
                    typeNames.Add(fullTypeName);
                }

                foreach (string name in typeNames)
                {
                    if (!resolverDict.ContainsKey(name))
                        return (model, error | NetworkMethodAnalysisError.MissingResolver, paramNode, type);
                }

            }

            model = new NetworkMethodModel(symbol, typeNames.ToArray());

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
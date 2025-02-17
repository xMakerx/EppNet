/////////////////////////////////////////////
/// Filename: Globals.cs
/// Date: February 8, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using EppNet.SourceGen.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        //////////////////////////////////////////////
        /// Descriptors
        ////////////////////////////////////////////// 
        
        /// <summary>
        /// Thrown when there is some kind of issue with a network type resolver
        /// </summary>
        public static DiagnosticDescriptor DescDebug = new(
            id: "EPN001",
            title: "Analyzer Debug",
            messageFormat: "{0}",
            category: "SourceGenerator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor DescError = new(
            id: "EPN002",
            title: "Analyzer Error",
            messageFormat: "{0}",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// Thrown when there is some kind of issue with a network type resolver
        /// </summary>
        public static DiagnosticDescriptor DescTypeResolverError = new(
            id: "EPN003",
            title: "Network Type Resolver Error",
            messageFormat: "{0}",
            category: "SourceGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// Thrown when there is some kind of error with a network object
        /// </summary>
        public static DiagnosticDescriptor DescNetObjError = new(
            id: "EPN004",
            title: "Network Object Error",
            messageFormat: "{0}",
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

        public static (NetworkObjectModel?, NetworkObjectAnalysisError) LocateNetObject(SyntaxNode node, SemanticModel semModel, CancellationToken cancelToken = default)
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
                return (model, NetworkObjectAnalysisError.NotPartial);

            // Step 2: Ensure class inherits from INetworkObject
            if (!symbol.AllInterfaces.Any(s => s.Name == "INetworkObject"))
                return (model, NetworkObjectAnalysisError.LacksInheritance);

            model = new(symbol);
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
                error |= ResolverAnalysisError.LacksSingleton;

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

                        // We return a null model if we have some kind of error
                        if (error == ResolverAnalysisError.None)
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
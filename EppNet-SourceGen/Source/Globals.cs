/////////////////////////////////////////////
/// Filename: Globals.cs
/// Date: February 8, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using EppNet.SourceGen.Errors;
using EppNet.SourceGen.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public const string DistributionArgName = "Dist";

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

        public const string ErrorSymbolResolution = "Failed to resolve symbol information.";

        // Network resolver related error strings
        public const string ErrorNetResolverNotClass = "Network Resolvers must be classes!";
        public const string ErrorNetResolverInheritance = $"Resolvers must inherit from \"{TypeResolverFullGenericName}\"";
        public const string ErrorNetResolverNoSingleton = "Resolvers must define a public static singleton field or property \"Instance\"";

        // Network object related error strings
        public const string ErrorNetObjInheritance = $"\"Network Objects must inherit from {NetworkObjectInterfaceName}!\"";
        public const string ErrorNetObjNotClass = "Network Objects must be classes!";
        public const string ErrorNetObjNotPartial = "Network Objects must be partial classes!";
        public const string ErrorNetObjDistIncompat = "Distribution type is incompatible with base class \"{0}\"!";

        // Network method related error strings
        public const string ErrorNetMethodModifiers = "Methods must be public, non-abstract, and synchronous!";
        public const string ErrorNetMethodInvalidParamType = "\"{0}\" is not a valid network type or network object. Do you have a resolver?";

        public static readonly SymbolDisplayFormat DisplayFormat = new(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.None,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

        public static readonly List<AnalysisDiagnostic> EmptyDiagnostics = [];
        public static readonly EquatableList<NetworkParameterTypeModel> EmptyParameterTypes = [];

        private static string GetAttributeName(AttributeSyntax attr)
        {
            return attr.Name switch
            {
                IdentifierNameSyntax id => id.Identifier.Text,
                QualifiedNameSyntax q => q.Right.Identifier.Text,
                _ => attr.Name.ToString()
            };
        }

        public static bool HasAttribute(ClassDeclarationSyntax classNode, string attrName)
        {
            foreach (var attrList in classNode.AttributeLists)
                foreach (var attr in attrList.Attributes)
                    if (GetAttributeName(attr) == attrName)
                        return true;

            return false;
        }

        public static bool HasAttribute(MethodDeclarationSyntax methodNode, string attrName)
        {
            foreach (var attrList in methodNode.AttributeLists)
                foreach (var attr in attrList.Attributes)
                    if (GetAttributeName(attr) == attrName)
                        return true;

            return false;
        }

        /// <summary>
        /// Checks if the specified fully qualified type name is valid; meaning:<br/>
        /// 1. It's listed under <see cref="SupportedTypes"/>; or<br/>
        /// 2. It has a registered resolver; or<br/>
        /// 3. It is a registered network object.
        /// </summary>
        /// <param name="typeSyntax"></param>
        /// <param name="semModel"></param>
        /// <param name="typeName"></param>
        /// <param name="resolverDict"></param>
        /// <returns>A tuple denoting if the type is valid and if it's a network object<br/>
        /// (IsValid, IsNetObject)</returns>
        public static (bool, bool) IsValidTypeName(ExecutionContext context, TypeSyntax typeSyntax, SemanticModel semModel, string typeName)
        {

            (bool found, (bool isValid, bool isNetObject)) = context.GetType(typeSyntax);

            if (found)
                return (isValid, isNetObject);

            if (SupportedTypes.Contains(typeName) ||
                (context.Resolvers is not null && context.Resolvers.ContainsKey(typeName)))
                return context.CacheTypeAndReturn(typeSyntax, typeName);

            // Let's determine if the type is a network object
            INamedTypeSymbol typeSymbol = semModel.GetSymbolInfo(typeSyntax).Symbol as INamedTypeSymbol ??
                semModel.GetTypeInfo(typeSyntax).Type as INamedTypeSymbol;

            // Invalid if null
            if (typeSymbol == null)
                return context.CacheTypeAndReturn(typeSyntax, typeName, false, false);

            SyntaxReference synRef = typeSymbol.DeclaringSyntaxReferences.FirstOrDefault();

            if (synRef != null && synRef.GetSyntax() is ClassDeclarationSyntax classNode &&
                HasAttribute(classNode, NetObjectAttr))
                return context.CacheTypeAndReturn(typeSyntax, typeName, true, true);

            return context.CacheTypeAndReturn(typeSyntax, typeName, false, false);
        }

        public static (NetworkParameterTypeModel?, TypeSyntax) TryCreateParameterModel(ExecutionContext context, TypeSyntax type, 
            SemanticModel semModel, CancellationToken cancelToken = default)
        {
            // We want to:
            // 1) Examine the type to see if it's supported.
            // - 1a: If it's a generic name, ensure the base type name is valid.
            // -     Enums, Dictionary, List, HashSet, SortedSet, and LinkedList are valid
            // 2) Check if the type has a resolver.
            NetworkParameterTypeModel? model = null;
            ITypeSymbol typeSymbol = semModel.GetTypeInfo(type, cancelToken).Type;

            if (cancelToken.IsCancellationRequested || typeSymbol == null)
                return (model, null);

            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                // This is an enum type. These always have an integral underlying type
                ITypeSymbol underlyingType = ((INamedTypeSymbol)typeSymbol).EnumUnderlyingType;
                string underlyingTypeName = context.GetTypeName(underlyingType);

                model = new NetworkParameterTypeModel(typeSymbol, null, underlyingType: underlyingTypeName);
                context.CacheType(type, underlyingTypeName);

                return (model, null);
            }

            if (type is ArrayTypeSyntax arrayType)
            {
                ITypeSymbol underlyingTypeSymbol = semModel.GetTypeInfo(arrayType.ElementType, cancelToken).Type;
                (bool isValidType, bool isNetObj) = IsValidTypeName(context, arrayType.ElementType, semModel, context.GetTypeName(underlyingTypeSymbol));

                if (!isValidType)
                    return (model, type);

                model = new NetworkParameterTypeModel(typeSymbol, null, context.GetTypeName(underlyingTypeSymbol), isNetObj);
                return (model, null);
            }

            if (type is GenericNameSyntax genericName)
            {
                string baseTypeName = $"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}";
                (bool isValidType, bool isNetObj) = IsValidTypeName(context, type, semModel, baseTypeName);

                if (!isValidType)
                    return (model, type);

                EquatableList<NetworkParameterTypeModel> subtypes = new();
                foreach (TypeSyntax typeArg in genericName.TypeArgumentList.Arguments)
                {
                    var result = TryCreateParameterModel(context, typeArg, semModel);

                    // Let's ensure the result is valid
                    if (result.Item1 is null)
                        return (model, typeArg);

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
                    var result = TryCreateParameterModel(context, typeArg, semModel);

                    // Let's ensure the result is valid
                    if (result.Item1 == null)
                        return (model, result.Item2);

                    subtypes.Add(result.Item1.Value);
                }

                model = new NetworkParameterTypeModel(typeSymbol, subtypes, null, isNetObject: false, isTuple: true);
                context.CacheType(type, context.GetTypeName(typeSymbol), true, false);
            }
            else
            {
                string fullTypeName = context.GetTypeName(typeSymbol);
                (bool isValidType, bool isNetObj) = IsValidTypeName(context, type, semModel, fullTypeName);

                if (!isValidType)
                    return (model, type);

                model = new NetworkParameterTypeModel(typeSymbol, null, isNetObject: isNetObj);
            }

            return (model, null);
        }

        /// <summary>
        /// Locates the distribution type from a specified <see cref="INamedTypeSymbol"/><br/>
        /// Returns -1 if the provided symbol does not have the <see cref="NetObjectAttrFullName"/> attribute.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private static (int, AttributeData) GetDistribution(INamedTypeSymbol symbol)
        {
            AttributeData attrData = symbol.GetAttributes().FirstOrDefault(
                attr => attr.AttributeClass?.ToDisplayString() == NetObjectAttrFullName);

            // If attribute not found, return -1
            if (attrData == null)
                return (-1, null);

            int distType = attrData!.ConstructorArguments.Length > 0 &&
                attrData!.ConstructorArguments[3].Value is int dist ? dist : 0;

            var namedArg = attrData!.NamedArguments.FirstOrDefault(arg => arg.Key == DistributionArgName);

            if (attrData.NamedArguments.FirstOrDefault(arg => arg.Key == DistributionArgName) is { Value.Value: int namedDistType })
                return (namedDistType, attrData);

            return (distType, attrData);
        }

        public static (NetworkObjectModel?, List<AnalysisDiagnostic>) TryCreateNetObject(ExecutionContext context, 
            CSharpSyntaxNode node, SemanticModel semModel, CancellationToken cancelToken = default)
        {
            // We want to:
            // 1) Ensure the class derives from INetworkObject
            // 2) Ensure the class is partial
            // 3) Examine the base class to ensure distribution types are compatible
            // 4) Try to create models of located network methods

            NetworkObjectModel? model = null;
            DiagnosticList allDiags = new(context);

            if (cancelToken.IsCancellationRequested)
                return (model, EmptyDiagnostics);

            if (node is not ClassDeclarationSyntax classNode)
                allDiags.Add(new AnalysisDiagnostic(DescNetObjError, node, ErrorNetObjNotClass));
            else
            {
                INamedTypeSymbol symbol = semModel.GetDeclaredSymbol(classNode);

                if (symbol == null)
                {
                    allDiags.Add(new AnalysisDiagnostic(DescNetObjError, node, ErrorSymbolResolution));
                    return (model, allDiags.Get());
                }

                // Step 1: Ensure class is partial
                if (!classNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                    allDiags.Add(new AnalysisDiagnostic(DescNetObjError, node, ErrorNetObjNotPartial));

                // Step 2: Ensure class derives from INetworkObject
                if (!symbol.AllInterfaces.Any(s => s.Name == NetworkObjectInterfaceName))
                    allDiags.Add(new AnalysisDiagnostic(DescNetObjError, node, ErrorNetObjInheritance));

                (int, AttributeData) distData = GetDistribution(symbol);
                int distType = distData.Item1;

                // Step 3: Examine base classes to check for network objects and ensure distr type compatibility
                INamedTypeSymbol baseSymbol = symbol.BaseType;

                INamedTypeSymbol tSymbol = baseSymbol;
                List<string> baseNetObjs = [];

                // Let's ascend the hierarchy
                while (true)
                {
                    int tDistType = GetDistribution(tSymbol).Item1;

                    if (tDistType != -1)
                    {
                        // This is a network object
                        string baseClassName = context.GetTypeName(tSymbol);
                        baseNetObjs.Add(baseClassName);

                        AttributeSyntax attrSyntax = distData.Item2.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;

                        if (!(tDistType == 0 || (tDistType == distType)))
                            allDiags.Add(new AnalysisDiagnostic(DescNetObjError, attrSyntax, string.Format(ErrorNetObjDistIncompat, baseClassName)));

                        tSymbol = tSymbol.BaseType;
                        continue;
                    }

                    // This is not a network object. We're done with our traversal
                    baseNetObjs?.Reverse();
                    break;
                }

                if (cancelToken.IsCancellationRequested)
                    return (model, allDiags.Get());

                // Step 4: Let's locate network methods
                (EquatableDictionary<string, EquatableHashSet<NetworkMethodModel>> myMethods, List<AnalysisDiagnostic> methodErrors) = 
                    TryAndLocateNetMethods(context, classNode, semModel);

                allDiags.AddRange(methodErrors);

                if (!allDiags.IsValueCreated)
                    model = new NetworkObjectModel(symbol, distType, new(baseNetObjs), myMethods);
            }

            return (model, allDiags.Get());
        }

        public static (NetworkMethodModel?, List<AnalysisDiagnostic>) TryCreateNetMethod(ExecutionContext context, MethodDeclarationSyntax methodNode, 
            SemanticModel semModel, CancellationToken cancelToken = default)
        {
            // Ensure the method has our attribute. If it doesn't...
            // we do not care
            if (cancelToken.IsCancellationRequested || methodNode == null || !HasAttribute(methodNode, NetMethodAttr))
                return (null, null);

            DiagnosticList allDiags = new(context);
            IMethodSymbol symbol = semModel.GetDeclaredSymbol(methodNode);

            if (symbol == null)
                return (null, [new AnalysisDiagnostic(DescNetMethodError, methodNode, ErrorSymbolResolution)]);

            // Step 1: Ensure the method is accessible and doesn't have invalid modifiers
            bool accessible = false;
            List<Location> invalidTokenLocations = null;

            foreach (SyntaxToken token in methodNode.Modifiers)
            {
                if (token.IsKind(SyntaxKind.PublicKeyword))
                    accessible = true;

                // Let's check for invalid modifiers
                if (token.IsKind(SyntaxKind.ProtectedKeyword) ||
                    token.IsKind(SyntaxKind.InternalKeyword) ||
                    token.IsKind(SyntaxKind.PrivateKeyword) ||
                    token.IsKind(SyntaxKind.AbstractKeyword) ||
                    token.IsKind(SyntaxKind.AsyncKeyword))
                {
                    if (invalidTokenLocations == null)
                        invalidTokenLocations = [];

                    invalidTokenLocations.Add(token.GetLocation());
                }

            }

            if (!accessible || invalidTokenLocations != null)
                allDiags.Add(new AnalysisDiagnostic(DescNetMethodError, methodNode,
                    ErrorNetMethodModifiers,
                    invalidTokenLocations));

            // Step 3: Ensure the parameters types have a network resolver or are
            // network objects
            Lazy<EquatableList<NetworkParameterTypeModel>> parameters = new(() => []);

            bool invalid = false;

            foreach (ParameterSyntax paramNode in methodNode.ParameterList.Parameters)
            {
                TypeSyntax type = paramNode.Type;

                (NetworkParameterTypeModel? typeModel, TypeSyntax typeArg) = TryCreateParameterModel(context, type, semModel, cancelToken);

                if (!typeModel.HasValue)
                {
                    ITypeSymbol typeSymbol = semModel.GetTypeInfo(typeArg, cancelToken).Type;
                    string typeName = context.GetTypeName(typeSymbol);

                    if (type is ArrayTypeSyntax arrayType)
                        allDiags.Add(new AnalysisDiagnostic(DescNetMethodError, typeArg,
                            string.Format(ErrorNetMethodInvalidParamType, context.GetTypeName(semModel.GetTypeInfo(arrayType.ElementType, cancelToken).Type), methodNode.GetLocation())));
                    else
                        allDiags.Add(new AnalysisDiagnostic(DescNetMethodError, typeArg, 
                            string.Format(ErrorNetMethodInvalidParamType, typeName), methodNode.GetLocation()));

                    invalid = true;
                    break;
                }
                else
                    parameters.Value.Add(typeModel.Value);
            }

            NetworkMethodModel? model = null;

            if (invalid)
                return (null, allDiags.Get());

            // If we didn't encounter any errors, let's create the model
            model = new(symbol, 
                parameters.IsValueCreated ?
                parameters.Value :
                EmptyParameterTypes);

            return (model, EmptyDiagnostics);
        }

        public static (EquatableDictionary<string, EquatableHashSet<NetworkMethodModel>>, List<AnalysisDiagnostic>) TryAndLocateNetMethods(
            ExecutionContext context, ClassDeclarationSyntax classNode, 
            SemanticModel semModel, CancellationToken cancelToken = default)
        {

            if (cancelToken.IsCancellationRequested)
                return (null, null);

            EquatableDictionary<string, EquatableHashSet<NetworkMethodModel>> methodsDict = new();
            DiagnosticList allDiags = new(context);

            foreach (var member in classNode.Members)
            {
                if (member is not MethodDeclarationSyntax methodNode)
                    continue;

                (NetworkMethodModel? model, List<AnalysisDiagnostic> diags) =
                    TryCreateNetMethod(context, methodNode, semModel, cancelToken);

                if (model.HasValue)
                {
                    string methodName = methodNode.Identifier.Text;
                    if (methodsDict.TryGetValue(methodName, out EquatableHashSet<NetworkMethodModel> list))
                        list.Add(model.Value);
                    else
                        methodsDict[methodName] = [model.Value];
                }

                allDiags.AddRange(diags);
            }

            return (methodsDict, allDiags.Get());
        }
        
        public static (ResolverModel?, List<AnalysisDiagnostic>) TryCreateResolver(bool analysisMode, ClassDeclarationSyntax classNode, SemanticModel semModel, 
            CancellationToken cancelToken = default)
        {
            // We want to:
            // 1) Ensure the class inherits from Resolver<T>
            // 2) Ensure the class has a public singleton symbol defined at the top
            ResolverModel? model = null;
            DiagnosticList allDiags = new(analysisMode);

            if (cancelToken.IsCancellationRequested)
                return (model, allDiags.Get());

            INamedTypeSymbol symbol = semModel.GetDeclaredSymbol(classNode);

            if (symbol == null)
                return (model, allDiags.AddAndGet(new AnalysisDiagnostic(DescTypeResolverError, classNode, ErrorSymbolResolution)));

            // Step 1: Ensure class has singleton field/property named Instance
            ISymbol singletonSymbol = symbol.GetMembers("Instance").FirstOrDefault(m =>
                m.IsStatic &&
                m.DeclaredAccessibility == Accessibility.Public &&
                ((m is IFieldSymbol field && field.Type.Name == symbol.Name) ||
                (m is IPropertySymbol prop && prop.Type.Name == symbol.Name))
            );

            // If we didn't locate it, indicate we're missing a singleton
            if (singletonSymbol == null)
                return (model, allDiags.AddAndGet(new AnalysisDiagnostic(DescTypeResolverError, classNode, ErrorNetResolverNoSingleton)));

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
                allDiags.Add(new AnalysisDiagnostic(DescTypeResolverError, classNode, ErrorNetResolverInheritance));

            return (model, allDiags.Get());
        }

    }

}
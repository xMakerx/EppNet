/////////////////////////////////////////////
/// Filename: NetworkObjectAnalysis.cs
/// Date: February 9, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using EppNet.SourceGen;
using EppNet.SourceGen.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;

namespace EppNet.Source.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NetworkObjectAnalysis : DiagnosticAnalyzer
    {

        public static ConcurrentDictionary<string, string> Resolvers = new();
        public static ConcurrentDictionary<string, NetworkObjectModel?> Objects = new();

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();


            context.RegisterCompilationStartAction(compContext =>
            {

                compContext.RegisterSyntaxNodeAction(snac =>
                {

                    ClassDeclarationSyntax classNode = snac.Node as ClassDeclarationSyntax;

                    if (!Globals.HasAttribute(classNode, Globals.NetTypeResolverAttr))
                        return;

                    (ResolverModel?, ResolverAnalysisError) data = Globals.LocateResolver(snac.Node, snac.SemanticModel, snac.CancellationToken);
                    ResolverAnalysisError results = data.Item2;

                    foreach (ResolverAnalysisError error in Enum.GetValues(typeof(ResolverAnalysisError)))
                    {
                        if (error == ResolverAnalysisError.None)
                            continue;

                        if (results.HasFlag(error))
                        {
                            string message = error switch
                            {
                                ResolverAnalysisError.LacksSingleton => $"{classNode.Identifier.Text}: Resolvers must define a public static singleton field or property \"Instance\"",
                                ResolverAnalysisError.LacksInheritance => $"{classNode.Identifier.Text}: Resolvers must inherit from {Globals.TypeResolverFullGenericName}",
                                ResolverAnalysisError.NotClass => $"{classNode.Identifier.Text}: Resolvers must be a class!",
                                _ => ToString(),
                            };
                            snac.ReportDiagnostic(Diagnostic.Create(Globals.DescTypeResolverError,
                                classNode.Identifier.GetLocation(), message));
                        }
                    }

                    if (results == ResolverAnalysisError.None && data.Item1.HasValue)
                    {
                        Resolvers[data.Item1.Value.ResolvedTypeFullName] = data.Item1.Value.Name;
                    }

                }, SyntaxKind.ClassDeclaration);

                compContext.RegisterSyntaxNodeAction(snac =>
                {
                    ClassDeclarationSyntax classNode = snac.Node as ClassDeclarationSyntax;

                    if (!Globals.HasAttribute(classNode, Globals.NetObjectAttr))
                        return;

                    var data = Globals.LocateNetObject(
                        classNode, snac.SemanticModel, snac.CancellationToken);

                    NetworkObjectModel? netModel = data.Item1;
                    NetworkObjectAnalysisError results = data.Item2;

                    foreach (NetworkObjectAnalysisError error in Enum.GetValues(typeof(NetworkObjectAnalysisError)))
                    {
                        if (error == NetworkObjectAnalysisError.None)
                            continue;

                        if (results.HasFlag(error))
                        {
                            string message = error switch
                            {
                                NetworkObjectAnalysisError.NotPartial => $"{classNode.Identifier.Text}: Network Object definitions must be partial! Num Resolvers: {Resolvers.Count}",
                                NetworkObjectAnalysisError.LacksInheritance => $"{classNode.Identifier.Text}: Network Object definitions must inherit from {Globals.NetworkObjectInterfaceName}",
                                NetworkObjectAnalysisError.NotClass => $"{classNode.Identifier.Text}: Network Object definitions must be a class!",
                                _ => ToString(),
                            };

                            snac.ReportDiagnostic(Diagnostic.Create(Globals.DescNetObjError,
                                classNode.Identifier.GetLocation(), message));
                        }
                    }

                    if (results == NetworkObjectAnalysisError.None && data.Item1.HasValue)
                    {
                        Objects[data.Item1.Value.FullyQualifiedName] = data.Item1;
                    }

                }, SyntaxKind.ClassDeclaration);

                compContext.RegisterSyntaxNodeAction(snac =>
                {

                    MethodDeclarationSyntax methodNode = snac.Node as MethodDeclarationSyntax;

                    if (!Globals.HasAttribute(methodNode, Globals.NetMethodAttr))
                        return;

                    var data = Globals.LocateNetMethod(methodNode, snac.SemanticModel, Resolvers, Objects);
                    NetworkMethodAnalysisError results = data.Item2;

                    foreach (NetworkMethodAnalysisError error in Enum.GetValues(typeof(NetworkMethodAnalysisError)))
                    {
                        if (error == NetworkMethodAnalysisError.None)
                            continue;

                        if (results.HasFlag(error))
                        {

                            if (error == NetworkMethodAnalysisError.MissingResolver)
                            {
                                ITypeSymbol typeSymbol = snac.SemanticModel.GetTypeInfo(data.Item4).Type;

                                if (typeSymbol == null)
                                    continue;

                                string fullTypeName = $"{typeSymbol.ContainingNamespace.ToDisplayString()}.{typeSymbol.Name}";

                                snac.ReportDiagnostic(Diagnostic.Create(Globals.DescNetMethodError,
                                    data.Item3.GetLocation(), [data.Item4.GetLocation()], $"{methodNode.Identifier.Text}: Type {fullTypeName} does not have a registered Resolver!"));

                                continue;
                            }

                            string message = error switch
                            {
                                NetworkMethodAnalysisError.NotNetworkObjectClass => $"{methodNode.Identifier.Text}: Network method must be within a class decorated with the NetworkObject attribute!",
                                NetworkMethodAnalysisError.Inaccessible => $"{methodNode.Identifier.Text}: Network method must be public!",
                                _ => $"{ToString()}",
                            };

                            snac.ReportDiagnostic(Diagnostic.Create(Globals.DescNetMethodError,
                                methodNode.Identifier.GetLocation(), message));
                        }
                    }

                }, SyntaxKind.MethodDeclaration);
            });


        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(Globals.DescTypeResolverError, Globals.DescNetObjError, Globals.DescNetMethodError);

    }

}

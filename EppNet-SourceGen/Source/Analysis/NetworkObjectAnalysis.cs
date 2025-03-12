/////////////////////////////////////////////
/// Filename: NetworkObjectAnalysis.cs
/// Date: February 9, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using EppNet.SourceGen;
using EppNet.SourceGen.Errors;
using EppNet.SourceGen.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EppNet.Source.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NetworkObjectAnalysis : DiagnosticAnalyzer
    {

        public static ConcurrentDictionary<string, string> Resolvers = new();
        public static ConcurrentDictionary<string, NetworkObjectModel> Objects = new();

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

                    (ResolverModel? model, List<AnalysisDiagnostic> errors) = Globals.TryCreateResolver(snac.Node as CSharpSyntaxNode, snac.SemanticModel, snac.CancellationToken);

                    foreach (AnalysisDiagnostic error in errors)
                        snac.ReportDiagnostic(error.CreateDiagnostic());

                    if (model.HasValue && errors.Count == 0)
                        Resolvers[model.Value.ResolvedTypeFullName] = model.Value.Name;

                }, SyntaxKind.ClassDeclaration);

                compContext.RegisterSyntaxNodeAction(snac =>
                {
                    ClassDeclarationSyntax classNode = snac.Node as ClassDeclarationSyntax;

                    if (!Globals.HasAttribute(classNode, Globals.NetObjectAttr))
                        return;

                    (NetworkObjectModel? netModel, List<AnalysisDiagnostic> errors) = Globals.TryCreateNetObject(
                        classNode, snac.SemanticModel, Resolvers, snac.CancellationToken);

                    foreach (AnalysisDiagnostic error in errors)
                        snac.ReportDiagnostic(error.CreateDiagnostic());

                    if (netModel.HasValue && errors.Count == 0)
                        Objects[netModel.Value.FullyQualifiedName] = netModel.Value;

                }, SyntaxKind.ClassDeclaration);

                compContext.RegisterSyntaxNodeAction(snac =>
                {

                    MethodDeclarationSyntax methodNode = snac.Node as MethodDeclarationSyntax;

                    if (methodNode is not null && !Globals.HasAttribute(methodNode, Globals.NetMethodAttr))
                        return;

                    (NetworkMethodModel? model, List<AnalysisDiagnostic> errors)
                        = Globals.TryCreateNetMethod(methodNode, snac.SemanticModel, Resolvers, snac.CancellationToken);

                    foreach (AnalysisDiagnostic error in errors)
                        snac.ReportDiagnostic(error.CreateDiagnostic());

                }, SyntaxKind.MethodDeclaration);
            });

        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(Globals.DescTypeResolverError, Globals.DescNetObjError, Globals.DescNetMethodError);

    }

}

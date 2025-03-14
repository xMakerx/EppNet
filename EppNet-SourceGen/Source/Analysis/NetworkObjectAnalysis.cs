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
        public static ExecutionContext Context { get; } = new ExecutionContext(true);

        public override void Initialize(AnalysisContext context)
        {

            // Let's setup our resolvers dictionary
            Context.Resolvers = new ConcurrentDictionary<string, string>();

            ConcurrentQueue<(ClassDeclarationSyntax, SemanticModel)> objects = new();
            ConcurrentQueue<(MethodDeclarationSyntax, SemanticModel)> methods = new();

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compContext =>
            {

                compContext.RegisterSyntaxNodeAction(snac =>
                {

                    ClassDeclarationSyntax classNode = snac.Node as ClassDeclarationSyntax;

                    if (!Globals.HasAttribute(classNode, Globals.NetTypeResolverAttr))
                        return;

                    (ResolverModel? model, List<AnalysisDiagnostic> diagnostics) = Globals.TryCreateResolver(true, classNode, snac.SemanticModel, snac.CancellationToken);

                    foreach (AnalysisDiagnostic diag in diagnostics)
                        snac.ReportDiagnostic(diag.CreateDiagnostic());

                    if (model.HasValue && diagnostics.Count == 0)
                        Context.Resolvers[model.Value.ResolvedTypeFullName] = model.Value.Name;

                }, SyntaxKind.ClassDeclaration);

                compContext.RegisterSyntaxNodeAction(snac =>
                {
                    ClassDeclarationSyntax classNode = snac.Node as ClassDeclarationSyntax;

                    if (!Globals.HasAttribute(classNode, Globals.NetObjectAttr))
                        return;

                    objects.Enqueue((classNode, snac.SemanticModel));

                }, SyntaxKind.ClassDeclaration);

                compContext.RegisterSyntaxNodeAction(snac =>
                {

                    MethodDeclarationSyntax methodNode = snac.Node as MethodDeclarationSyntax;

                    if (methodNode is not null && !Globals.HasAttribute(methodNode, Globals.NetMethodAttr))
                        return;

                    // Don't recalculate the methods of a network object
                    if (methodNode.Parent is not null &&
                        methodNode.Parent is ClassDeclarationSyntax classNode &&
                        Globals.HasAttribute(classNode, Globals.NetObjectAttr))
                        return;

                    methods.Enqueue((methodNode, snac.SemanticModel));

                }, SyntaxKind.MethodDeclaration);

                compContext.RegisterCompilationEndAction(snac =>
                {

                    while (objects.Count > 0)
                    {
                        objects.TryDequeue(out var result);
                        (ClassDeclarationSyntax classNode, SemanticModel semanticModel) = result;

                        (NetworkObjectModel? netModel, List<AnalysisDiagnostic> diagnostics) = Globals.TryCreateNetObject(
                            Context, classNode, semanticModel);

                        foreach (AnalysisDiagnostic diag in diagnostics)
                            snac.ReportDiagnostic(diag.CreateDiagnostic());
                    }

                    while (methods.Count > 0)
                    {
                        methods.TryDequeue(out var result);
                        (MethodDeclarationSyntax methodNode, SemanticModel semanticModel) = result;

                        foreach (AnalysisDiagnostic diag in Globals.TryCreateNetMethod(Context, methodNode, semanticModel).Item2)
                            snac.ReportDiagnostic(diag.CreateDiagnostic());
                    }
                });
            });

        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(Globals.DescTypeResolverError, Globals.DescNetObjError, Globals.DescNetMethodError);

    }

}

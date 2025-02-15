/////////////////////////////////////////////
/// Filename: ResolverAnalysis.cs
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
using System.Collections.Immutable;

namespace EppNet.Source.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ResolverAnalysis : DiagnosticAnalyzer
    {

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction((snac) =>
            {

                ClassDeclarationSyntax classNode = snac.Node as ClassDeclarationSyntax;

                if (!Globals.HasAttribute(classNode, Globals.NetTypeResolverAttr))
                    return;

                ResolverAnalysisError results = Globals.LocateResolver(snac.Node, snac.SemanticModel, snac.CancellationToken).Item2;

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

            }, SyntaxKind.ClassDeclaration);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Globals.DescTypeResolverError);

    }

}

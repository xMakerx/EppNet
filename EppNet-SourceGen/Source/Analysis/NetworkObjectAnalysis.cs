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
    public class NetworkObjectAnalysis : DiagnosticAnalyzer
    {

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction((snac) =>
            {

                ClassDeclarationSyntax classNode = snac.Node as ClassDeclarationSyntax;

                if (!Globals.HasAttribute(classNode, Globals.NetObjectAttr))
                    return;

                NetworkObjectAnalysisError results = Globals.LocateNetObject(snac.Node, snac.SemanticModel, snac.CancellationToken).Item2;

                foreach (NetworkObjectAnalysisError error in Enum.GetValues(typeof(NetworkObjectAnalysisError)))
                {
                    if (error == NetworkObjectAnalysisError.None)
                        continue;

                    if (results.HasFlag(error))
                    {
                        string message = error switch
                        {
                            NetworkObjectAnalysisError.NotPartial => $"{classNode.Identifier.Text}: Network Object definitions must be partial!",
                            NetworkObjectAnalysisError.LacksInheritance => $"{classNode.Identifier.Text}: Network Object definitions must inherit from {Globals.NetworkObjectInterfaceName}",
                            NetworkObjectAnalysisError.NotClass => $"{classNode.Identifier.Text}: Network Object definitions must be a class!",
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

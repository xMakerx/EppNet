using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

/*
[Generator]
public class ResolverGenerator : IIncrementalGenerator
{

    static DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: "GEN001",
            title: "Source Generator Debug Message",
            messageFormat: "{0}",
            category: "SourceGenerator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get all class declarations
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations =
            context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                .Where(static cls => cls.BaseList != null); // Ensure it has a base class

        // Combine the syntax tree with the compilation model
        IncrementalValueProvider<Compilation> compilationProvider =
            context.CompilationProvider;

        // Register code generation logic
        context.RegisterSourceOutput(compilationProvider.Combine(classDeclarations.Collect()),
            (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classDeclarations, SourceProductionContext context)
    {
        foreach (var classSyntax in classDeclarations)
        {
            var model = compilation.GetSemanticModel(classSyntax.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;

            if (classSymbol == null)
                continue;

            // Traverse the base type chain
            INamedTypeSymbol baseType = classSymbol.BaseType;
            while (baseType != null)
            {
                if (baseType.OriginalDefinition.ToDisplayString() == "EppNet.Data.Resolver<T>")
                {
                    // Extract type argument
                    if (baseType.IsGenericType && baseType.TypeArguments.Length == 1)
                    {
                        ITypeSymbol genericType = baseType.TypeArguments[0];
                        string genericTypeName = genericType.ToDisplayString();

                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, classSyntax.GetLocation(),
                            $"Class {classSymbol.Name} derives from Resolver<{genericTypeName}>"));
                    }

                    break;
                }

                baseType = baseType.BaseType;
            }
        }
    }
}
*/
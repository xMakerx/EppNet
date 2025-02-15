/////////////////////////////////////////////
/// Filename: ContextHelper.cs
/// Date: January 31, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace EppNet.SourceGen
{
    public struct ContextHelper
    {

        public GeneratorAttributeSyntaxContext? AttributeContext { get; }
        public GeneratorSyntaxContext? Context { get; }

        public ClassDeclarationSyntax ClassDeclaration { get; }
        public SemanticModel SemanticModel { get; }

        public INamedTypeSymbol ClassSymbol { get; }

        public ContextHelper(GeneratorSyntaxContext? ctx = null, GeneratorAttributeSyntaxContext? attrCtx = null)
        {
            if (ctx.HasValue)
            {
                Context = ctx;

                if (Context.Value.Node is ClassDeclarationSyntax classDecl)
                    ClassDeclaration = classDecl;

                SemanticModel = Context.Value.SemanticModel;
                //ClassSymbol = ClassDeclaration != null ? SemanticModel.GetDeclaredSymbol(ClassDeclaration) as INamedTypeSymbol : null;
            }
            else
            {
                AttributeContext = attrCtx;

                if (AttributeContext.Value.TargetNode is ClassDeclarationSyntax classDecl)
                    ClassDeclaration = classDecl;

                SemanticModel = AttributeContext.Value.SemanticModel;
                ClassSymbol = ClassDeclaration != null ? SemanticModel.GetDeclaredSymbol(ClassDeclaration) as INamedTypeSymbol : null;
            }
        }

        public bool IsAttributeContext() => AttributeContext.HasValue;

        public AttributeData GetAttributeByName(string attrName)
        {
            // Haven't implemented for non attribute context
            if (!IsAttributeContext())
                return null;

            return AttributeContext.Value.Attributes
                .FirstOrDefault(attr => attr.AttributeClass.Name == attrName);
        }

    }
}

/////////////////////////////////////////////
/// Filename: ClassData.cs
/// Date: January 31, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;


namespace EppNet.SourceGen
{
    public struct ClassData
    {
        public ContextHelper Context { get; }
        public ClassDeclarationSyntax ClassDeclaration { get; }
        public SemanticModel SemanticModel { get; }
        public INamedTypeSymbol ClassSymbol { get; }
        public AttributeData AttributeData { set; get; }

        public INamedTypeSymbol BaseClassSymbol { set; get; }

        public ClassData(ContextHelper context)
        {
            this.Context = context;
            this.ClassDeclaration = context.ClassDeclaration;
            this.SemanticModel = context.SemanticModel;
            this.ClassSymbol = context.ClassSymbol;
            this.AttributeData = null;
        }

    }

}

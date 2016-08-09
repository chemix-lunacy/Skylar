using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skyling.Core.Parser.TreeWalkers
{
    public class ConstructorWalker : MethodWalker
    {
        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            Identity = semanticModel.GetDeclaredSymbol(node);
            base.VisitConstructorDeclaration(node);
        }
    }
}

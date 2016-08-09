using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skyling.Core.Parser.TreeWalkers
{
    public class OperatorWalker : MethodWalker
    {
        public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            Identity = semanticModel.GetDeclaredSymbol(node);
            base.VisitOperatorDeclaration(node);
        }
    }
}

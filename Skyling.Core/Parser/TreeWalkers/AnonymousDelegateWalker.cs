using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skyling.Core.Parser.TreeWalkers
{
    public class AnonymousDelegateWalker : MethodWalker
    {
        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            Identity = semanticModel.GetDeclaredSymbol(node);
            base.VisitAnonymousMethodExpression(node);
        }
    }
}

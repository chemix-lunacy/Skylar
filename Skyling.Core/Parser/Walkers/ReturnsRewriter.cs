using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skyling.Core.Parser.Walkers
{
    public abstract class ReturnsRewriter : CSharpSyntaxRewriter
    {
        protected bool ShouldRewrite(ReturnStatementSyntax ret)
        {
            if (ret == null)
                return false;
            
            ExpressionSyntax returnExpression = ret.Expression;
            return ret.Expression != null && !(returnExpression is IdentifierNameSyntax);
        }
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skyling.Core.Parser
{
    /// <summary>
    /// Expands single-line return expressions into blocked expressions, as we need them in that form later.
    /// 
    /// So:
    /// 
    /// if (...)
    ///     return;
    ///     
    /// Becomes:
    /// 
    /// if (...) 
    /// {
    ///     return;
    /// }
    /// </summary>
    public class ExpandReturnsRewriter : ReturnsRewriter
    {
        /// <summary>
        /// Helper method for all situations where we have a non-block return value that may or may not be required to be modified.
        /// If the single-statement needs rewriting we'll return the newly-created single-statement block, otherwise the original statement.
        /// </summary>
        private StatementSyntax RewriteReturnNode(StatementSyntax singleStatement)
        {
            ReturnStatementSyntax retStatement = singleStatement as ReturnStatementSyntax;
            if (retStatement != null && ShouldRewrite(retStatement))
            {
                return SyntaxFactory.Block(retStatement);
            }

            return singleStatement;
        }

        public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
        {
            return base.VisitForEachStatement(node.ReplaceNode(node.Statement, RewriteReturnNode(node.Statement)));
        }

        public override SyntaxNode VisitElseClause(ElseClauseSyntax node)
        {
            return base.VisitElseClause(node.ReplaceNode(node.Statement, RewriteReturnNode(node.Statement)));
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            return base.VisitIfStatement(node.ReplaceNode(node.Statement, RewriteReturnNode(node.Statement)));
        }
    }
}

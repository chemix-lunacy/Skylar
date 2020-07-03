using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Skyling.Core.Parser.Walkers
{
    /// <summary>
    /// To make things consistent we want to turn all return expressions into assignments followed by returns, so we can uniformly deal with output connections as symbols to expression chains.
    /// Ex:
    ///     return a + b + c;
    ///     
    /// Turns into:
    ///     int ret = a + b + c;
    ///     return ret;
    /// </summary>
    public class SplitReturnsRewriter : ReturnsRewriter
    {
        public SplitReturnsRewriter(SemanticModel sm) => SemanticModel = sm;

        private SemanticModel SemanticModel { get; set; }

        private int retValCount = 0;

        public static string ReturnVariableName = "_returnValue";

        private IEnumerable<StatementSyntax> BuildReturnStatements(ReturnStatementSyntax ret)
        {
            IdentifierNameSyntax variableName = SyntaxFactory.IdentifierName(ReturnVariableName + retValCount++);
            VariableDeclaratorSyntax varDec = SyntaxFactory.VariableDeclarator(variableName.Identifier, null,
                SyntaxFactory.EqualsValueClause(ret.Expression));

            TypeInfo info = SemanticModel.GetTypeInfo(ret.Expression);
            VariableDeclarationSyntax varDecSyntax = SyntaxFactory.VariableDeclaration(
                SyntaxFactory.ParseTypeName(info.Type.ToMinimalDisplayString(SemanticModel, 0)),
                new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(varDec));
            LocalDeclarationStatementSyntax localDec = SyntaxFactory.LocalDeclarationStatement(varDecSyntax);
            ReturnStatementSyntax rewrittenReturn = ret.WithExpression(variableName);

            return new List<StatementSyntax>{ localDec, rewrittenReturn };
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            List<StatementSyntax> bodyStatements = new List<StatementSyntax>(node.Statements);
            IEnumerable<ReturnStatementSyntax> modifableReturns = node.Statements.OfType<ReturnStatementSyntax>().Where(val => ShouldRewrite(val));
            foreach (ReturnStatementSyntax ret in modifableReturns)
            {
                int retIndex = bodyStatements.IndexOf(ret);
                bodyStatements.RemoveAt(retIndex);
                bodyStatements.InsertRange(retIndex, BuildReturnStatements(ret));
            }

            if (modifableReturns.Any())
            {
                BlockSyntax rewrittenBlock = node.WithStatements(new SyntaxList<StatementSyntax>(bodyStatements));
                return base.VisitBlock(rewrittenBlock.NormalizeWhitespace());
            }

            return base.VisitBlock(node);
        }
    }
}

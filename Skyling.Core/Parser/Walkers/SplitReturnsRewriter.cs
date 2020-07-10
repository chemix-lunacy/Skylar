using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

using RewrittenReturnsList = System.Collections.Generic.List<(Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax MatchingReturn, System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> ReplacementStatements)>;

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
            ITypeSymbol typeSymbol = info.Type;
            if (typeSymbol == null)
                return new List<StatementSyntax>();

            VariableDeclarationSyntax varDecSyntax = SyntaxFactory.VariableDeclaration(
                SyntaxFactory.ParseTypeName(typeSymbol.ToMinimalDisplayString(SemanticModel, 0)),
                new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(varDec));
            LocalDeclarationStatementSyntax localDec = SyntaxFactory.LocalDeclarationStatement(varDecSyntax);
            ReturnStatementSyntax rewrittenReturn = ret.WithExpression(variableName);

            return new List<StatementSyntax>{ localDec, rewrittenReturn };
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            RewrittenReturnsList rewrittenReturns = new RewrittenReturnsList(
                node.Statements.OfType<ReturnStatementSyntax>().Where(ret => ShouldRewrite(ret)).Select(ret => (ret, BuildReturnStatements(ret))));

            SyntaxNode childNode = base.VisitBlock(node);
            if (!(childNode is BlockSyntax childBlock))
                return childNode;

            List<StatementSyntax> bodyStatements = new List<StatementSyntax>(childBlock.Statements);
            foreach (ReturnStatementSyntax ret in childBlock.Statements.OfType<ReturnStatementSyntax>())
            {
                // TODO: Hack way to do it, but after a nested block has been re-written all nodes have been orphaned, so we can't do type 
                // resolution. Fix later if there's a nicer way to do it.
                var linkedReturn = rewrittenReturns.FirstOrDefault(val => val.MatchingReturn.ToFullString() == ret.ToFullString());
                if (linkedReturn != default) 
                {
                    int index = bodyStatements.IndexOf(ret);
                    bodyStatements.RemoveAt(index);
                    bodyStatements.InsertRange(index, linkedReturn.ReplacementStatements);
                }
            }

            if (bodyStatements.Count != childBlock.Statements.Count)
                return childBlock.WithStatements(new SyntaxList<StatementSyntax>(bodyStatements)).NormalizeWhitespace();

            return childNode;
        }
    }
}

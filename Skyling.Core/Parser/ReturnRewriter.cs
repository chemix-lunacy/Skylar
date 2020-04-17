using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Parser
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
    public class ReturnRewriter
    {
        private int retValCount = 0;

        public SyntaxNode Rewrite(SemanticModel semanticModel, SyntaxNode root) {

            if (root == null)
                return null;

            IEnumerable<ReturnStatementSyntax> returns = root.DescendantNodes().OfType<ReturnStatementSyntax>();
            SyntaxNode rewrittenRoot = root.TrackNodes(returns);
            foreach (ReturnStatementSyntax node in returns) 
            {
                TypeInfo info = semanticModel.GetTypeInfo(node.Expression);
                if (info.Type != null && !(node.Expression is IdentifierNameSyntax))
                {
                    IdentifierNameSyntax variableName = SyntaxFactory.IdentifierName("_returnValue" + retValCount++);
                    VariableDeclaratorSyntax varDec = SyntaxFactory.VariableDeclarator(variableName.Identifier, null,
                        SyntaxFactory.EqualsValueClause(node.Expression));

                    VariableDeclarationSyntax varDecSyntax = SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName(info.Type.ToMinimalDisplayString(semanticModel, 0)),
                        new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(varDec));
                    LocalDeclarationStatementSyntax localDec = SyntaxFactory.LocalDeclarationStatement(varDecSyntax);

                    ReturnStatementSyntax movedReturn = rewrittenRoot.GetCurrentNode(node);
                    rewrittenRoot = rewrittenRoot.InsertNodesBefore(movedReturn ?? node, new[] { localDec });
                    movedReturn = rewrittenRoot.GetCurrentNode(node);
                    rewrittenRoot = rewrittenRoot.ReplaceNode(movedReturn, movedReturn.WithExpression(variableName));
                }
            }

            return rewrittenRoot;
        }
    }
}

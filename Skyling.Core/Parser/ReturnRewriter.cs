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

            //root.

            List<MethodDeclarationSyntax> classMethods = new List<MethodDeclarationSyntax>();
            foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (method.Body == null)
                {
                    //classMethods.Add(method);
                    continue;
                }

                List<StatementSyntax> bodyStatements = new List<StatementSyntax>(method.Body.Statements);
                IEnumerable<ReturnStatementSyntax> returns = method.Body.Statements.OfType<ReturnStatementSyntax>();
                foreach (ReturnStatementSyntax ret in returns)
                {
                    ExpressionSyntax returnExpression = ret.Expression;
                    TypeInfo info = semanticModel.GetTypeInfo(returnExpression);
                    if (info.Type != null && !(returnExpression is IdentifierNameSyntax))
                    {
                        IdentifierNameSyntax variableName = SyntaxFactory.IdentifierName("_returnValue" + retValCount++);
                        VariableDeclaratorSyntax varDec = SyntaxFactory.VariableDeclarator(variableName.Identifier, null,
                            SyntaxFactory.EqualsValueClause(returnExpression));

                        VariableDeclarationSyntax varDecSyntax = SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.ParseTypeName(info.Type.ToMinimalDisplayString(semanticModel, 0)),
                            new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(varDec));
                        LocalDeclarationStatementSyntax localDec = SyntaxFactory.LocalDeclarationStatement(varDecSyntax);
                        ReturnStatementSyntax rewrittenReturn = ret.WithExpression(variableName);

                        int retIndex = bodyStatements.IndexOf(ret);
                        bodyStatements.RemoveAt(retIndex);
                        bodyStatements.Insert(retIndex, rewrittenReturn);
                        bodyStatements.Insert(retIndex, localDec);
                    }
                }

                if (returns.Any())
                    root = root.ReplaceNode(method, method.WithBody(method.Body.WithStatements(new SyntaxList<StatementSyntax>(bodyStatements))));

                //if (returns.Any())
                //    classMethods.Add(method.WithBody(method.Body.WithStatements(bodyStatements)));
                //else
                //    classMethods.Add(method);
            }

            //root.

            root = root.NormalizeWhitespace();

            return root;
        }
    }
}

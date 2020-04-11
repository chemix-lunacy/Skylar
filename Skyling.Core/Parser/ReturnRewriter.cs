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
    public class ReturnRewriter : CSharpSyntaxRewriter
    {
        public ReturnRewriter(SemanticModel semModel, SyntaxNode oldRoot)
        {
            semanticModel = semModel;
            modifiedTree = oldRoot;
        }

        SemanticModel semanticModel;

        int retValCount = 0;

        SyntaxNode modifiedTree;

        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
        {
            TypeInfo info = semanticModel.GetTypeInfo(node.Expression);
            if (info.Type != null && !(node.Expression is IdentifierNameSyntax))
            {
                IdentifierNameSyntax variableName = SyntaxFactory.IdentifierName("_returnValue" + retValCount++);
                VariableDeclaratorSyntax varDec = SyntaxFactory.VariableDeclarator(variableName.Identifier, null,
                    SyntaxFactory.EqualsValueClause(node.Expression));

                modifiedTree.TrackNodes(new []{ node });
                ReturnStatementSyntax movedReturn1 = modifiedTree.GetCurrentNode(node);

                //SyntaxFactory.GenericName(info.Type.Name)
                VariableDeclarationSyntax varDecSyntax = SyntaxFactory.VariableDeclaration(SyntaxFactory.GenericName(info.Type.Name), 
                    new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(varDec));
                LocalDeclarationStatementSyntax localDec = SyntaxFactory.LocalDeclarationStatement(varDecSyntax);
                modifiedTree = modifiedTree.InsertNodesBefore(node, new[] { localDec });
                ReturnStatementSyntax movedReturn = modifiedTree.GetCurrentNode(node);
                modifiedTree = modifiedTree.ReplaceNode(movedReturn, movedReturn.WithExpression(variableName));

                //node.WithExpression(SyntaxFactory)
                //SyntaxFactory.AssignmentExpression
                //node.with
            }

            //SyntaxFactory.DeclarationExpression(node.Expression)
            //SyntaxFactory.AssignmentExpression
            //node.with

            return base.VisitReturnStatement(node);
        }
    }
}

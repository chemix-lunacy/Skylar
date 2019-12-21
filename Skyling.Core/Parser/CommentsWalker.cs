using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Skyling.Core.Parser.TreeWalkers
{
    public class CommentsWalker : CSharpSyntaxWalker
    {
        public CommentsWalker() : base(Microsoft.CodeAnalysis.SyntaxWalkerDepth.StructuredTrivia) { }

        private Dictionary<SyntaxNode, List<string>> comments = new Dictionary<SyntaxNode, List<string>>();

        public SyntaxNode GetApplicableSyntaxNode(SyntaxTrivia syntaxTriv)
        {
            return GetApplicableSyntaxNode(syntaxTriv.Token.Parent);
        }

        public SyntaxNode GetApplicableSyntaxNode(StructuredTriviaSyntax structuredTrivia) 
        {
            return GetApplicableSyntaxNode(structuredTrivia?.ParentTrivia.Token.Parent);
        }

        public SyntaxNode GetApplicableSyntaxNode(SyntaxNode node)
        {
            if (node == null)
                return null;

            var currentParent = node;
            while (currentParent != null)
            {
                if (currentParent.IsKind(SyntaxKind.ClassDeclaration)
                    || currentParent.IsKind(SyntaxKind.MethodDeclaration)
                    || currentParent.IsKind(SyntaxKind.EnumDeclaration)
                    || currentParent.IsKind(SyntaxKind.ConstructorDeclaration)
                    || currentParent.IsKind(SyntaxKind.DestructorDeclaration)
                    || currentParent.IsKind(SyntaxKind.FieldDeclaration)
                    || currentParent.IsKind(SyntaxKind.StructDeclaration)
                    || currentParent.IsKind(SyntaxKind.PropertyDeclaration))
                {
                    return currentParent;
                }

                currentParent = currentParent.Parent;
            }

            return null;
        }

        public void AddComments(SyntaxNode targetNode, params string[] comments)
        {
            AddComments(targetNode, comments.ToList());
        }

        public void AddComments(SyntaxNode targetNode, IEnumerable<string> appliedComments)
        {
            if (appliedComments == null || !appliedComments.Any() || targetNode == null)
                return;

            List<string> commentsList;
            if (!comments.TryGetValue(targetNode, out commentsList))
            {
                commentsList = comments[targetNode] = new List<string>();
            }

            commentsList.AddRange(appliedComments);
        }

        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                AddComments(GetApplicableSyntaxNode(trivia), 
                    trivia.ToFullString().Replace("///", ""));
            }

            base.VisitTrivia(trivia);
        }

        public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
        {
            SyntaxNode synt = GetApplicableSyntaxNode(node);
            if (synt != null) 
            {
                List<string> commentsList = new List<string>();
                var xml = XElement.Parse("<fakeRoot>" + node.ToFullString().Replace("///", "") + "</fakeRoot>");
                foreach (XElement val in xml.Elements("summary"))
                {
                    commentsList.Add(val.Value.Trim());
                }

                AddComments(synt, commentsList);
            }

            base.VisitDocumentationCommentTrivia(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            if (node != null)
            {
                AddComments(GetApplicableSyntaxNode(node), node.Identifier.ValueText);
            }

            base.VisitVariableDeclarator(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            SyntaxNode syntaxNode = GetApplicableSyntaxNode(node);

            AddComments(syntaxNode, node.Identifier.ValueText);
            foreach (ParameterSyntax paramSyn in node.ParameterList.Parameters) 
            {
                AddComments(syntaxNode, paramSyn.Identifier.ValueText);
            }

            base.VisitMethodDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            SyntaxNode syntaxNode = GetApplicableSyntaxNode(node);
            AddComments(syntaxNode, node.Identifier.ValueText);

            base.VisitPropertyDeclaration(node);
        }
    }
}

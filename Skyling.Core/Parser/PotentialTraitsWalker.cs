using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Skyling.Core.Concepts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Skyling.Core.Parser.TreeWalkers
{
    /// <summary>
    /// Attempts to work out traits from a syntax tree.
    /// 
    /// Method Trait: Attempt at contextualization of what the method does, a combination of comments and operations.
    /// Value Trait: Variable name / method call name (if available). If operations are involved add those as a text, such as 
    /// name + surname will result in ("add", "name", "surname").
    /// </summary>
    public class PotentialTraitsWalker : SkylingWalker
    {
        public PotentialTraitsWalker(SemanticModel sm, TraitsStorage storage) : base(SyntaxWalkerDepth.StructuredTrivia)
        { 
            semanticModel = sm;
            this.traits = storage;
        }

        private Dictionary<SyntaxNode, HashSet<string>> comments = new Dictionary<SyntaxNode, HashSet<string>>();

        private SemanticModel semanticModel;

        private TraitsStorage traits;

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
                    || currentParent.IsKind(SyntaxKind.PropertyDeclaration)
                    || currentParent.IsKind(SyntaxKind.VariableDeclarator))
                {
                    return currentParent;
                }

                currentParent = currentParent.Parent;
            }

            return currentParent;
        }

        #region Comments

        private void AddComments(SyntaxNode targetNode, params string[] comments)
        {
            AddComments(targetNode, comments.ToList());
        }

        private void AddComments(SyntaxNode targetNode, IEnumerable<string> appliedComments)
        {
            if (appliedComments == null || !appliedComments.Any() || targetNode == null)
                return;

            HashSet<string> commentsList;
            if (!comments.TryGetValue(targetNode, out commentsList))
            {
                commentsList = comments[targetNode] = new HashSet<string>();
            }

            commentsList.UnionWith(appliedComments);
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

        #endregion

        /// <summary>
        /// Take an expression and expand it into a resultant TraitsSet. Does not modify existing trait sets, just aggregates them.
        /// </summary>
        private TraitsSet ExpandExpression(SyntaxNode node) 
        {
            TraitsSet GetTraitsFromNode(SyntaxNode syntaxNode) {
                SymbolInfo symb = semanticModel.GetSymbolInfo(syntaxNode);
                return symb.Symbol != null ? traits.GetTraits(symb.Symbol) : new TraitsSet();
            }

            TraitsSet sourceSet = new TraitsSet();
            if (node is InvocationExpressionSyntax invocation)
            {
                var methodProspect = semanticModel.GetSymbolInfo(invocation);
                if (methodProspect.Symbol is IMethodSymbol methodSymbol)
                {
                    sourceSet = traits.GetTraits(methodSymbol);
                    sourceSet.Add(methodSymbol.Name);
                    sourceSet.Add(methodSymbol.ContainingType.Name);
                }
            }

            if (node is IdentifierNameSyntax identifier)
            {
                sourceSet = GetTraitsFromNode(identifier);
                sourceSet.Add(identifier.Identifier.ValueText);
            }

            if (node is BinaryExpressionSyntax binaryExpr) 
            {
                sourceSet = ExpandExpression(binaryExpr.Left).Union(ExpandExpression(binaryExpr.Right));
                sourceSet.Add(binaryExpr.OperatorToken.ValueText);
            }

            return new TraitsSet(sourceSet);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            if (node != null)
            {
                ISymbol symb = semanticModel.GetDeclaredSymbol(node);
                if (!symb.Name.StartsWith(SplitReturnsRewriter.ReturnVariableName, StringComparison.Ordinal))
                    traits.AddSymbolTrait(symb, symb.Name);
                else
                    traits.AddSymbolTrait(symb, symb.ContainingSymbol.Name);

                traits.AddSymbolTrait(symb, ExpandExpression(node.Initializer.Value));
            }

            base.VisitVariableDeclarator(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(node);
            if (methodSymbol != null) 
                traits.AddSymbolTrait(methodSymbol, node.Identifier.ValueText);

            foreach (ParameterSyntax paramSyn in node.ParameterList.Parameters) 
            {
                var symbolInfo = semanticModel.GetDeclaredSymbol(paramSyn);
                if (symbolInfo != null)
                    traits.AddSymbolTrait(symbolInfo, paramSyn.Identifier.ValueText);
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

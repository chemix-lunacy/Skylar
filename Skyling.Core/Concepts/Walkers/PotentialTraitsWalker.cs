using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Skyling.Core.Concepts;
using Skyling.Core.Decompilation;
using Skyling.Core.Parser;
using Skyling.Core.Parser.Walkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Skyling.Core.Concepts.Walkers
{
    /// <summary>
    /// Attempts to work out traits from a syntax tree.
    /// 
    /// Method Trait: Attempt at contextualization of what the method does, a combination of comments and operations.
    /// Value Trait: Variable name / method call name (if available). If operations are involved add those as a text, such as 
    /// name + surname will result in ("add", "name", "surname").
    /// </summary>
    public class PotentialTraitsWalker : CSharpSyntaxWalker
    {
        public PotentialTraitsWalker(SemanticModel sm, TraitResolver storage) : base(SyntaxWalkerDepth.StructuredTrivia)
        { 
            semanticModel = sm;
            traits = storage;
        }

        private HashSet<string> orphanedComments = new HashSet<string>();

        private Dictionary<SyntaxNode, HashSet<string>> comments = new Dictionary<SyntaxNode, HashSet<string>>();

        private SemanticModel semanticModel;

        private TraitResolver traits;

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
                orphanedComments.Add(
                    trivia.ToFullString().Replace("///", ""));
            }
            else if (trivia.HasStructure) 
            {
                AddComments(trivia.GetStructure(),
                    trivia.ToFullString().Replace("///", ""));
            }

            base.VisitTrivia(trivia);
        }

        public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
        {
            if (node != null) 
            {
                List<string> commentsList = new List<string>();
                var xml = XElement.Parse("<fakeRoot>" + node.ToFullString().Replace("///", "") + "</fakeRoot>");
                foreach (XElement val in xml.Elements("summary"))
                {
                    commentsList.Add(val.Value.Trim());
                }

                AddComments(node, commentsList);
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
            if (node == null)
                return sourceSet;

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
                    traits.AddSymbolTrait(symb, symb.ContainingSymbol?.Name);

                traits.AddSymbolTrait(symb, ExpandExpression(node.Initializer?.Value));
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

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symb = semanticModel.GetSymbolInfo(node);
            if (symb.Symbol != null)
                traits.GetTraits(symb.Symbol);

            base.VisitInvocationExpression(node);
        }
    }
}

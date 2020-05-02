using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Skyling.Core.Concepts
{
    public class TraitDatabase
    {
        public TraitDatabase(SemanticModel semModel) => semanticModel = semModel; 

        private SemanticModel semanticModel;
        private Dictionary<ISymbol, TraitCollection> symbolTraits = new Dictionary<ISymbol, TraitCollection>();
        private Dictionary<SyntaxToken, TraitCollection> methodTraits = new Dictionary<SyntaxToken, TraitCollection>();

        public void PropogateTraits(MethodDeclarationSyntax methodDecl)
        {
            this.PropogateTraits(methodDecl, new HashSet<SyntaxToken>());
        }

        public void PropogateTraits(MethodDeclarationSyntax methodDecl, HashSet<SyntaxToken> recursionGuard)
        {
            if (methodDecl == null || recursionGuard.Contains(methodDecl.Identifier) || methodTraits.ContainsKey(methodDecl.Identifier))
                return;

            recursionGuard.Add(methodDecl.Identifier);
            foreach (ParameterSyntax param in methodDecl.ParameterList.Parameters)
            {
                var argsList = param.AttributeLists
                        .SelectMany(val => val.Attributes)
                        .Where(val => val.Name.ToString() == "Trait")
                        .Where(val => val.ArgumentList != null)
                        .SelectMany(val => val.ArgumentList.Arguments)
                        .Select(val => val.ToString().Trim('"'));

                IParameterSymbol symbInf = this.semanticModel.GetDeclaredSymbol(param);
                if (argsList.Any() && symbInf != null)
                {
                    symbolTraits.Add(symbInf, new TraitCollection(argsList));
                }
            }

            // TODO: Fix typing for TraitAttribute check.
            List<string> methodAttributes = new List<string>();
            List<string> returnAttributes = new List<string>();
            foreach (AttributeListSyntax attriList in methodDecl.AttributeLists)
            {
                AttributeTargetSpecifierSyntax target = attriList.Target;

                IEnumerable<string> attries = attriList.Attributes
                    .Where(val => val.Name.ToString() == "Trait")
                    .Where(val => val.ArgumentList != null)
                    .SelectMany(val => val.ArgumentList.Arguments)
                    .Select(val => val.ToString().Trim('"'));

                // TODO: Match against proper syntax, even though this also works.
                if (target == null)
                    methodAttributes.AddRange(attries);
                else if (target.Identifier.ValueText == "return")
                    returnAttributes.AddRange(attries);
            }

            IMethodSymbol methSymb = this.semanticModel.GetDeclaredSymbol(methodDecl);
            if (methSymb != null)
            {
                foreach (MethodDeclarationSyntax methodImplementations in methSymb.DeclaringSyntaxReferences
                    .Select(val => val.GetSyntax()).OfType<MethodDeclarationSyntax>())
                {
                    this.GetTraits(methodImplementations);
                }
            }

            methodTraits.Add(methodDecl.Identifier, new TraitCollection(methodAttributes));
            foreach (SymbolInfo returnSymbol in methodDecl.DescendantNodes().OfType<ReturnStatementSyntax>()
                     .Select(val => this.semanticModel.GetSymbolInfo(val.Expression)).Where(val => val.Symbol != null))
                this.symbolTraits.Add(returnSymbol.Symbol, new TraitCollection(returnAttributes));

            foreach (AssignmentExpressionSyntax assign in methodDecl.DescendantNodes().OfType<AssignmentExpressionSyntax>())
            {
                if (assign.Left is IdentifierNameSyntax ident) 
                {
                    SymbolInfo symbolInfo = this.semanticModel.GetSymbolInfo(ident);
                    if (symbolInfo.Symbol != null && assign.Right is InvocationExpressionSyntax rhs)
                    {
                        //TraitCollection existingTraits, rhsTraits;
                        //if (methodTraits.TryGetValue(rhs, out rhsTraits)) 
                        //{ 
                        //    if (!symbolTraits.TryGetValue(symbolInfo.Symbol, out existingTraits))
                        //    {
                        //        existingTraits = new TraitCollection();
                        //        symbolTraits.Add(symbolInfo.Symbol, existingTraits);
                        //    }

                        //    existingTraits.Traits.AddRange(rhsTraits.Traits);
                        //}
                    }
                }
            }
        }

        public TraitCollection GetTraits(MethodDeclarationSyntax methodNode)
        {
            return GetTraits(methodNode.Identifier);
        }

        public TraitCollection GetTraits(SyntaxToken token)
        {
            return this.methodTraits.TryGetValue(token, out TraitCollection traits)
                ? traits
                : new TraitCollection();
        }

        public TraitCollection GetTraits(ISymbol symb)
        {
            return this.symbolTraits.TryGetValue(symb, out TraitCollection traits)
                ? traits
                : new TraitCollection();
        }

        public TraitCollection GetTraits(CSharpSyntaxNode node) 
        {
            return new TraitCollection();
        }
    }
}

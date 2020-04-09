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
        private Dictionary<MethodDeclarationSyntax, TraitCollection> methodTraits = new Dictionary<MethodDeclarationSyntax, TraitCollection>();
        private Dictionary<ReturnStatementSyntax, TraitCollection> returnTraits = new Dictionary<ReturnStatementSyntax, TraitCollection>();

        public void PropogateTraits(MethodDeclarationSyntax methodDecl)
        {
            if (methodDecl == null)
                return;

            foreach (ParameterSyntax param in methodDecl.ParameterList.Parameters)
            {
                var argsList = param.AttributeLists
                        .SelectMany(val => val.Attributes)
                        .Where(val => val.Name.ToString() == "Trait")
                        .Where(val => val.ArgumentList != null)
                        .SelectMany(val => val.ArgumentList.Arguments)
                        .Select(val => val.ToString().Trim('"'));

                SymbolInfo symbInf = this.semanticModel.GetSymbolInfo(param);
                if (argsList.Any() && symbInf.Symbol != null)
                {
                    symbolTraits.Add(symbInf.Symbol, new TraitCollection(argsList));
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

            methodTraits.Add(methodDecl, new TraitCollection(methodAttributes));
            ReturnWalker walker = new ReturnWalker();
            walker.Visit(methodDecl);
            foreach (ReturnStatementSyntax returnStatement in walker.Returns)
                returnTraits.Add(returnStatement, new TraitCollection(returnAttributes));
        }

        private class ReturnWalker : CSharpSyntaxWalker 
        {
            public List<ReturnStatementSyntax> Returns { get; set; } = new List<ReturnStatementSyntax>();
            
            public override void VisitReturnStatement(ReturnStatementSyntax node)
            {
                Returns.Add(node);
                base.VisitReturnStatement(node);
            }
        }

        public TraitCollection GetTraits(ReturnStatementSyntax returnNode)
        {
            return this.returnTraits.TryGetValue(returnNode, out TraitCollection traits) 
                ? traits
                : new TraitCollection();
        }

        public TraitCollection GetTraits(MethodDeclarationSyntax methodNode)
        {
            return this.methodTraits.TryGetValue(methodNode, out TraitCollection traits)
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

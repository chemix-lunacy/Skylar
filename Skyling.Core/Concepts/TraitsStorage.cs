using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Skyling.Core.Concepts
{
    public class TraitsStorage
    {
        public TraitsStorage() { }

        public Dictionary<ISymbol, MethodTraitsStorage> MethodTraits { get; } = new Dictionary<ISymbol, MethodTraitsStorage>();

        //public void PropogateTraits(MethodDeclarationSyntax methodDecl)
        //{
        //    this.PropogateTraits(methodDecl, new HashSet<SyntaxToken>());
        //}

        //public void PropogateTraits(MethodDeclarationSyntax methodDecl, HashSet<SyntaxToken> recursionGuard)
        //{
        //    if (methodDecl == null || recursionGuard.Contains(methodDecl.Identifier) || methodTraits.ContainsKey(methodDecl.Identifier))
        //        return;

        //    recursionGuard.Add(methodDecl.Identifier);
        //    foreach (ParameterSyntax param in methodDecl.ParameterList.Parameters)
        //    {
        //        var argsList = param.AttributeLists
        //            .SelectMany(val => val.Attributes)
        //            .Where(val => val.Name.ToString() == "Trait")
        //            .Where(val => val.ArgumentList != null)
        //            .SelectMany(val => val.ArgumentList.Arguments)
        //            .Select(val => val.ToString().Trim('"'));

        //        IParameterSymbol symbInf = this.semanticModel.GetDeclaredSymbol(param);
        //        if (argsList.Any() && symbInf != null)
        //        {
        //            symbolTraits.Add(symbInf, new TraitsSet(argsList));
        //        }
        //    }

        //    // TODO: Fix typing for TraitAttribute check.
        //    List<string> methodAttributes = new List<string>();
        //    List<string> returnAttributes = new List<string>();
        //    foreach (AttributeListSyntax attriList in methodDecl.AttributeLists)
        //    {
        //        AttributeTargetSpecifierSyntax target = attriList.Target;

        //        IEnumerable<string> attries = attriList.Attributes
        //            .Where(val => val.Name.ToString() == "Trait")
        //            .Where(val => val.ArgumentList != null)
        //            .SelectMany(val => val.ArgumentList.Arguments)
        //            .Select(val => val.ToString().Trim('"'));

        //        // TODO: Match against proper syntax, even though this also works.
        //        if (target == null)
        //            methodAttributes.AddRange(attries);
        //        else if (target.Identifier.ValueText == "return")
        //            returnAttributes.AddRange(attries);
        //    }

        //    IMethodSymbol methSymb = this.semanticModel.GetDeclaredSymbol(methodDecl);
        //    if (methSymb != null)
        //    {
        //        foreach (MethodDeclarationSyntax methodImplementations in methSymb.DeclaringSyntaxReferences
        //            .Select(val => val.GetSyntax()).OfType<MethodDeclarationSyntax>())
        //        {
        //            this.GetTraits(methodImplementations);
        //        }
        //    }

        //    methodTraits.Add(methodDecl.Identifier, new TraitsSet(methodAttributes));
        //    foreach (SymbolInfo returnSymbol in methodDecl.DescendantNodes().OfType<ReturnStatementSyntax>().Where(val => val.Expression != null)
        //             .Select(val => this.semanticModel.GetSymbolInfo(val.Expression)).Where(val => val.Symbol != null))
        //        this.symbolTraits.Add(returnSymbol.Symbol, new TraitsSet(returnAttributes));

        //    foreach (AssignmentExpressionSyntax assign in methodDecl.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        //    {
        //        if (assign.Left is IdentifierNameSyntax ident) 
        //        {
        //            SymbolInfo symbolInfo = this.semanticModel.GetSymbolInfo(ident);
        //            if (symbolInfo.Symbol != null && assign.Right is InvocationExpressionSyntax rhs)
        //            {
        //                //TraitCollection existingTraits, rhsTraits;
        //                //if (methodTraits.TryGetValue(rhs, out rhsTraits)) 
        //                //{ 
        //                //    if (!symbolTraits.TryGetValue(symbolInfo.Symbol, out existingTraits))
        //                //    {
        //                //        existingTraits = new TraitCollection();
        //                //        symbolTraits.Add(symbolInfo.Symbol, existingTraits);
        //                //    }

        //                //    existingTraits.Traits.AddRange(rhsTraits.Traits);
        //                //}
        //            }
        //        }
        //    }
        //}

        public void AddSymbolTrait(ISymbol symbol, params string[] newTraits)
        {
            this.AddSymbolTrait(symbol, new TraitsSet(newTraits));
        }

        public void AddSymbolTrait(ISymbol symbol, TraitsSet traitSet)
        {
            if (symbol == null)
                return;

            if (!MethodTraits.TryGetValue(symbol.ContainingSymbol, out MethodTraitsStorage methodTraits))
                methodTraits = MethodTraits[symbol] = new MethodTraitsStorage();

            methodTraits.AddSymbolTrait(symbol, traitSet);
        }

        public TraitsSet GetTraits(ISymbol symbol)
        {
            return symbol != null && MethodTraits.TryGetValue(symbol.ContainingSymbol, out MethodTraitsStorage traits)
                ? traits.GetTraits(symbol)
                : new TraitsSet();
        }
    }
}

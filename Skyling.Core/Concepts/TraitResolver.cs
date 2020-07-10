using Microsoft.CodeAnalysis;
using Skyling.Core.Decompilation;
using Skyling.Core.Parser;
using System.Collections.Generic;

namespace Skyling.Core.Concepts
{
    public class TraitResolver
    {
        public TraitResolver(ConceptualAbstractionResolver concept) => conceptResolver = concept;

        private ConceptualAbstractionResolver conceptResolver;

        public Dictionary<ISymbol, MethodTraitsStorage> MethodTraits { get; } = new Dictionary<ISymbol, MethodTraitsStorage>();

        private IMethodSymbol GetMethodSymbol(ISymbol symb)
        {
            if (symb is IMethodSymbol)
                return symb as IMethodSymbol;

            if (symb.ContainingSymbol is IMethodSymbol)
                return symb.ContainingSymbol as IMethodSymbol;

            return null;
        }

        public void AddSymbolTrait(ISymbol symbol, params string[] newTraits)
        {
            this.AddSymbolTrait(symbol, new TraitsSet(newTraits));
        }

        // TODO: Fix duplicate symbols from (I believe) internal/external projects not being the same. Likely tricky, use primary solution to attempt to align all symbols? Unlikely.
        // TODO: Decide what to do with symbol collections. Currently some weird ones leak into the list like fields.
        public void AddSymbolTrait(ISymbol symbol, TraitsSet traitSet)
        {
            if (symbol == null)
                return;

            IMethodSymbol methodSymbol = GetMethodSymbol(symbol);
            if (methodSymbol == null)
                return;

            if (!MethodTraits.TryGetValue(methodSymbol ?? symbol, out MethodTraitsStorage methodTraits))
                methodTraits = MethodTraits[symbol] = new MethodTraitsStorage();

            methodTraits.AddSymbolTrait(symbol, traitSet);
        }

        public TraitsSet GetTraits(ISymbol symbol)
        {
            IMethodSymbol methodSymbol = GetMethodSymbol(symbol);
            if (methodSymbol == null)
                return new TraitsSet();

            if (!MethodTraits.ContainsKey(methodSymbol))
                conceptResolver.AnalyzeSymbol(methodSymbol);

            if (MethodTraits.TryGetValue(methodSymbol, out MethodTraitsStorage traits))
                return traits.GetTraits(symbol);
            else
                return new TraitsSet();
        }
    }
}

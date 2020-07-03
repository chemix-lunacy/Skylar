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

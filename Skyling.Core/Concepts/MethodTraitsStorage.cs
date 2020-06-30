using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Skyling.Core.Concepts
{
    [DebuggerDisplay("{MethodTraits,nq}")]
    public class MethodTraitsStorage
    {
        public TraitsSet MethodTraits { get => SymbolTraits.Values.Aggregate(new TraitsSet(), (existing, next) => existing.Union(next)); }

        public Dictionary<ISymbol, TraitsSet> SymbolTraits { get; set; } = new Dictionary<ISymbol, TraitsSet>();

        public void AddSymbolTrait(ISymbol symbol, params string[] newTraits)
        {
            if (!SymbolTraits.TryGetValue(symbol, out TraitsSet existingTraits))
                SymbolTraits.Add(symbol, existingTraits = new TraitsSet());

            existingTraits.Traits.UnionWith(newTraits);
        }

        public void AddSymbolTrait(ISymbol symbol, TraitsSet traitSet)
        {
            if (!SymbolTraits.TryGetValue(symbol, out TraitsSet existingTraits))
                SymbolTraits.Add(symbol, existingTraits = new TraitsSet());

            existingTraits.Traits.UnionWith(traitSet.Traits);
        }

        public TraitsSet GetTraits(ISymbol symb)
        {
            return this.SymbolTraits.TryGetValue(symb, out TraitsSet traits)
                ? traits
                : this.SymbolTraits[symb] = new TraitsSet();
        }
    }
}

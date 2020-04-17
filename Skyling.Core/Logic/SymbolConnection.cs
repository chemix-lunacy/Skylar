using Microsoft.CodeAnalysis;
using Skyling.Core.Concepts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Logic
{
    [DebuggerDisplay("Connection: {this.Symbol}. Traits: {this.Traits}")]
    public class SymbolConnection
    {
        public SymbolConnection(ISymbol symb, TraitCollection traits) 
        {
            Symbol = symb;
            Traits = traits;
        }

        TraitCollection Traits { get; set; } = new TraitCollection();

        ISymbol Symbol { get; set; }
    }
}

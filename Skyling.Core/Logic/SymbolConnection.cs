using Microsoft.CodeAnalysis;
using Skyling.Core.Concepts;
using System.Diagnostics;

namespace Skyling.Core.Logic
{
    [DebuggerDisplay("Connection: {this.Symbol}. Traits: {this.Traits}")]
    public class SymbolConnection
    {
        public SymbolConnection(ISymbol symb, TraitsSet traits) 
        {
            Symbol = symb;
            Traits = traits;
        }

        TraitsSet Traits { get; set; } = new TraitsSet();

        ISymbol Symbol { get; set; }
    }
}

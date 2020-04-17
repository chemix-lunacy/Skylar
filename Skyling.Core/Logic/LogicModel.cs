using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Skyling.Core.Concepts;
using System.Collections.Generic;

namespace Skyling.Core.Logic
{
    public class LogicModel
    {
        public LogicModel(SemanticModel sm, DataFlowAnalysis dfa, ControlFlowGraph cfa, TraitCollection traits, params StatementSyntax[] statementSyntaxes)
        {
            SemanticModel = sm;
            DataFlowAnalysis = dfa;
            ControlFlowGraph = cfa;
            Traits = traits;
            Statements = new List<StatementSyntax>(statementSyntaxes);
        }

        public HashSet<ISymbol> UsedSymbols
        {
            get
            {
                HashSet<ISymbol> used = new HashSet<ISymbol>(this.DataFlowAnalysis.ReadInside);
                used.UnionWith(this.DataFlowAnalysis.WrittenInside);
                return used;
            }
        }

        public SemanticModel SemanticModel { get; set; }

        public DataFlowAnalysis DataFlowAnalysis { get; set; }

        public ControlFlowGraph ControlFlowGraph { get; set; }

        public List<StatementSyntax> Statements { get; set; } = new List<StatementSyntax>();

        public TraitCollection Traits { get; set; } = new TraitCollection();

        public ConnectionPoints ConectionPoints { get; set; }
    }
}

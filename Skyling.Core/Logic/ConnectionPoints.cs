using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Skyling.Core.Concepts;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Logic
{
    public class ConnectionPoints
    {
        public ConnectionPoints(LogicModel model, TraitsStorage traitDB) 
        {
            foreach (var connection in model.DataFlowAnalysis.DataFlowsIn
                    .Intersect(model.DataFlowAnalysis.ReadInside)
                    .Select(val => new SymbolConnection(val, traitDB.GetTraits(val))))
                InputConnections.Add(connection);

            foreach (var connection in model.DataFlowAnalysis.DataFlowsOut
                    .Select(val => new SymbolConnection(val, traitDB.GetTraits(val))))
                OutputConnections.Add(connection);

            foreach (var connection in model.Statements.OfType<ReturnStatementSyntax>().Select(val => 
                model.SemanticModel.GetSymbolInfo(val.Expression)).Where(val => val.Symbol != null)
                    .Select(val => new SymbolConnection(val.Symbol, traitDB.GetTraits(val.Symbol))))
                OutputConnections.Add(connection);
        }

        List<SymbolConnection> InputConnections { get; set; } = new List<SymbolConnection>();
        
        List<SymbolConnection> OutputConnections { get; set; } = new List<SymbolConnection>();

        public bool CanConnect(List<SymbolConnection> connetions)
        {
            return false;
        }
    }
}

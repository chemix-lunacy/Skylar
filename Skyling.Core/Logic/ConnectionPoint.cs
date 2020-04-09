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
    public class ConnectionPoint
    {
        public ConnectionPoint(LogicModel model, TraitDatabase traitDB) 
        {
            Contract.Requires(model != null);

            foreach (var connection in model.DataFlowAnalysis.DataFlowsIn
                    .Intersect(model.DataFlowAnalysis.ReadInside)
                    .Select(val => new SymbolConnection(val, traitDB.GetTraits(val))))
                InputConnections.Add(connection);

            foreach (var connection in model.DataFlowAnalysis.DataFlowsOut
                    .Select(val => new SymbolConnection(val, traitDB.GetTraits(val))))
                OutputConnections.Add(connection);

            foreach (var connection in model.Statements.OfType<ReturnStatementSyntax>()
                    .Select(val => new ReturnConnection(val, traitDB.GetTraits(val))))
                OutputConnections.Add(connection);

            LogicModel = model;
        }

        List<IInputConnection> InputConnections { get; set; } = new List<IInputConnection>();
        List<IOutputConnection> OutputConnections { get; set; } = new List<IOutputConnection>();

        LogicModel LogicModel { get; set; }
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Skyling.Core.Concepts;
using Skyling.Core.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Parser
{
    public class LogicModelWalker : SkylingWalker
    {
        public LogicModelWalker(SemanticModel model) => traitGen = new TraitDatabase(this.semanticModel = model);

        List<LogicModel> models = new List<LogicModel>();

        TraitDatabase traitGen;

        SemanticModel semanticModel;

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            traitGen.PropogateTraits(node);
            if (node.Body != null)
            {
                LogicModel lm = new LogicModel(
                    this.semanticModel,
                    this.semanticModel.AnalyzeDataFlow(node.Body), 
                    ControlFlowGraph.Create(node, this.semanticModel),
                    traitGen.GetTraits(node), 
                    node.Body.Statements.ToArray());
                lm.ConectionPoints = new ConnectionPoints(lm, this.traitGen);
                models.Add(lm);
            }

            base.VisitMethodDeclaration(node);
        }
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Skyling.Core.Concepts;
using Skyling.Core.Logic;
using Skyling.Core.Parser.Walkers;
using System.Collections.Generic;
using System.Linq;

namespace Skyling.Core.Logic.Walkers
{
    public class LogicModelWalker : SkylingWalker
    {
        public LogicModelWalker(SemanticModel model, TraitResolver traits)
        {
            traitGen = traits;
            semanticModel = model;
        }

        List<LogicModel> models = new List<LogicModel>();

        TraitResolver traitGen;

        SemanticModel semanticModel;

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Body != null)
            {
                IMethodSymbol methodSym = this.semanticModel.GetDeclaredSymbol(node);

                LogicModel lm = new LogicModel(
                    this.semanticModel,
                    this.semanticModel.AnalyzeDataFlow(node.Body), 
                    ControlFlowGraph.Create(node, this.semanticModel),
                    traitGen.GetTraits(methodSym), 
                    node.Body.Statements.ToArray());
                lm.ConectionPoints = new ConnectionPoints(lm, this.traitGen);
                models.Add(lm);
            }

            base.VisitMethodDeclaration(node);
        }
    }
}

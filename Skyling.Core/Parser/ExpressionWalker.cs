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
    public class ExpressionWalker : SkylingWalker
    {
        public ExpressionWalker(SemanticModel model) => traitGen = new TraitDatabase(this.semanticModel = model);

        List<ConnectionPoint> models = new List<ConnectionPoint>();

        TraitDatabase traitGen;

        SemanticModel semanticModel;

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            traitGen.PropogateTraits(node);
            if (node.Body != null)
            {
                TraitCollection traits = traitGen.GetTraits(node);
                models.Add(new ConnectionPoint(new LogicModel(this.semanticModel.AnalyzeDataFlow(node.Body), ControlFlowGraph.Create(node, this.semanticModel),
                    traits, node.Body.Statements.ToArray()), this.traitGen));
            }

            base.VisitMethodDeclaration(node);
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            base.VisitReturnStatement(node);
        }

        public override void VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            base.VisitExpressionStatement(node);
        }
    }
}

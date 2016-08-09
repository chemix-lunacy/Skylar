using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Skyling.Core.Parser.TreeWalkers
{
    public class MethodWalker : BaseLogicWalker
    {
        public List<AnonymousDelegateWalker> AnonymousMethods { get; } = new List<AnonymousDelegateWalker>();

        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            base.VisitContinueStatement(node);
        }

        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            base.VisitBreakStatement(node);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            base.VisitConditionalExpression(node);
        }

        public override void VisitYieldStatement(YieldStatementSyntax node)
        {
            base.VisitYieldStatement(node);
        }
        
        public override void VisitCheckedStatement(CheckedStatementSyntax node)
        {
            base.VisitCheckedStatement(node);
        }
        
        public override void VisitDoStatement(DoStatementSyntax node)
        {
            base.VisitDoStatement(node);
        }

        public override void VisitFixedStatement(FixedStatementSyntax node)
        {
            base.VisitFixedStatement(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            base.VisitForEachStatement(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            base.VisitForStatement(node);
        }

        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            base.VisitGotoStatement(node);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            //SemanticModel model = this.Compilation.GetSemanticModel(node.SyntaxTree);
            //ControlFlowAnalysis controlFlow = model.AnalyzeControlFlow(node.Statement);
            //DataFlowAnalysis dataFlow = model.AnalyzeDataFlow(node.Statement);

            base.VisitIfStatement(node);
        }

        public override void VisitLabeledStatement(LabeledStatementSyntax node)
        {
            base.VisitLabeledStatement(node);
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            base.VisitLocalDeclarationStatement(node);
        }

        public override void VisitLockStatement(LockStatementSyntax node)
        {
            base.VisitLockStatement(node);
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            base.VisitReturnStatement(node);
            
            if (node.Expression == null)
                return;

            //SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            //SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(node.Expression);

            //LogicPathCollection result = null;
            //if (symbolInfo.Symbol != null && expressionSyntaxMap.Contains(symbolInfo.Symbol))
            //{
            //    result = this.expressionSyntaxMap.Get(symbolInfo.Symbol);
            //}
            //else 
            //{
            //    result = this.Z3Utility.GetExpressionResult(node.Expression);
            //}

            //this.ReturnExpressions.Add(result);
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            base.VisitSwitchStatement(node);
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            base.VisitThrowStatement(node);
        }

        public override void VisitTryStatement(TryStatementSyntax node)
        {
            base.VisitTryStatement(node);
        }

        public override void VisitUnsafeStatement(UnsafeStatementSyntax node)
        {
            base.VisitUnsafeStatement(node);
        }

        public override void VisitUsingStatement(UsingStatementSyntax node)
        {
            base.VisitUsingStatement(node);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            base.VisitWhileStatement(node);
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            base.VisitLiteralExpression(node);
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            base.VisitVariableDeclaration(node);

            //SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            //foreach (VariableDeclaratorSyntax variable in node.Variables)
            //{
            //    ISymbol expressionSymbol = semanticModel.GetDeclaredSymbol(variable);
            //    if (expressionSymbol != null && variable.Initializer != null)
            //    {
            //        LogicPathCollection resultExpression = this.Z3Utility.GetExpressionResult(variable.Initializer.Value);
            //        expressionSyntaxMap.Add(expressionSymbol, resultExpression);
            //    }
            //}
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            base.VisitObjectCreationExpression(node);

            //ISymbol variable = this.GetAssociatedVariable(node);
            //AnalysisContext analysisContext = this.RoslynUtility.GetAnalysisContext(node);

            //MethodWalker walker = this.AnalysisComposite.AnalyzeMethod(analysisContext);

            //if (variable != null && walker != null)
            //{
            //     this.expressionSyntaxMap.Add(variable, walker.ReturnExpressions);
            //}
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            base.VisitAnonymousMethodExpression(node);

            //SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            //ISymbol expressionSymbol = semanticModel.GetSymbolInfo(node).Symbol;
            //this.Identity = expressionSymbol;

            //if (Identity != expressionSymbol)
            //{
            //    AnonymousDelegateWalker walker = this.CreateSyntaxWalker<AnonymousDelegateWalker>();
            //    walker.Visit(node);
            //    this.AnonymousMethods.Add(walker);
            //}
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            //SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            //this.Identity = semanticModel.GetDeclaredSymbol(node);

            base.VisitMethodDeclaration(node);
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            base.VisitAssignmentExpression(node);

            //SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);

            //ISymbol assignmentVariable = this.GetAssociatedVariable(node.Left);
            //LogicPathCollection resultExpression = this.Z3Utility.GetExpressionResult(node.Right);
            //if (resultExpression != null && assignmentVariable != null)
            //{
            //    expressionSyntaxMap.Add(assignmentVariable, resultExpression);
            //}
        }

        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            //SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            //this.Identity = semanticModel.GetDeclaredSymbol(node);

            //if (node.Kind() == SyntaxKind.GetAccessorDeclaration)
            //{
            //    // A no-body accessor is just that. In this case simply assign a variable of the type so we kinda know what's going on.
            //    if (node.Body == null)
            //    {
            //        PropertyDeclarationSyntax propertyDeclaration = node.Parent.Parent as PropertyDeclarationSyntax;
            //        TypeInfo info = semanticModel.GetTypeInfo(propertyDeclaration.Type);

            //        LogicPath path = new LogicPath();
            //        path.Expressions.Add(this.Z3Utility.CreateVariable(info.Type, propertyDeclaration.Identifier.ToString()));
            //    }
            //}
            
            base.VisitAccessorDeclaration(node);
        }
    }
}
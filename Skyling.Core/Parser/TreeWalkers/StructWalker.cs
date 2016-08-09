using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skyling.Core.Parser.TreeWalkers
{
    public class StructWalker : TypeWalker
    {
        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            Identity = semanticModel.GetDeclaredSymbol(node);
            base.VisitStructDeclaration(node);
        }
    }
}

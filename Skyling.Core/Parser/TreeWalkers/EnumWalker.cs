using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skyling.Core.Parser.TreeWalkers
{
    public class EnumWalker : TypeWalker
    {
        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            SemanticModel semanticModel = this.Compilation.GetSemanticModel(node.SyntaxTree);
            Identity = semanticModel.GetDeclaredSymbol(node);
            base.VisitEnumDeclaration(node);
        }
    }
}

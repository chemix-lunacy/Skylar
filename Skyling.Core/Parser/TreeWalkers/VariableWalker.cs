using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skyling.Core.Parser.TreeWalkers
{
    public class VariableWalker : BaseLogicWalker
    {
        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
        }
    }
}

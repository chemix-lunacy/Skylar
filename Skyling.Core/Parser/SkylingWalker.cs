using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Skyling.Core.Parser
{
    public class SkylingWalker : CSharpSyntaxWalker
    {
        public SkylingWalker() { }

        public SkylingWalker(SyntaxWalkerDepth structuredTrivia) : base(structuredTrivia) { }
    }
}

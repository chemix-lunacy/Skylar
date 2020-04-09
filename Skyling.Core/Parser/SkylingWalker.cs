using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Parser
{
    public class SkylingWalker : CSharpSyntaxWalker
    {
        public SkylingWalker() { }

        public SkylingWalker(SyntaxWalkerDepth structuredTrivia) : base(structuredTrivia) { }
    }
}

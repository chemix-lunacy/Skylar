using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Decompilation
{
    [DebuggerDisplay("{Assembly.FullName}")]
    public class DecompiledAssembly
    {
        public string Path { get; set; }

        public CSharpCompilation Compilation { get; set; }

        public ModuleDefinition ModuleDef { get; set; }

        public Assembly Assembly { get; set; }

        public Dictionary<string, SyntaxTree> SyntaxTrees { get; set; }
    }
}

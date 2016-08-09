using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Skyling.Core.Parser
{
    [DebuggerDisplay("{this.Identity ?? this.GetType().ToString()}")]
    public class AnalysisContext
    {
        private string Identity => this.TypeSymbol?.ToDisplayString() ?? this.GetType().ToString();
        
        public ITypeSymbol TypeSymbol { get; set; }

        public IMethodSymbol MethodSymbol { get; set; }
        
        private string assemblyPath;

        public string AssemblyPath { get { return assemblyPath ?? this.TypeSymbol?.ContainingAssembly.Identity.Name ?? ""; } set { this.assemblyPath = value; } }

        public override bool Equals(object obj)
        {
            AnalysisContext ident = obj as AnalysisContext;
            if (ident != null)
            {
                return this.TypeSymbol == ident.TypeSymbol && this.MethodSymbol == ident.MethodSymbol;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (this.AssemblyPath + (this.MethodSymbol?.Name ?? "") + (this.TypeSymbol?.Name ?? "")).GetHashCode();
        }
    }
}

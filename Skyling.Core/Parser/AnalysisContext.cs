using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Skyling.Core.Parser
{
    /// <summary>
    /// Test, can I see this?
    /// </summary>
    [DebuggerDisplay("{this.Identity ?? this.GetType().ToString()}")]
    public class AnalysisContext
    {
        private string Identity => this.TypeSymbol?.ToDisplayString() ?? this.GetType().ToString();

        // Some comment.

        /// <summary>
        /// Some field.
        /// </summary>
        public ITypeSymbol TypeSymbol { get; set; }

        public IMethodSymbol MethodSymbol { get; set; }

        private string assemblyPath;

        public string AssemblyPath { get { return assemblyPath ?? this.TypeSymbol?.ContainingAssembly.Identity.Name ?? ""; } set { this.assemblyPath = value; } }

        /// <summary>
        /// Method Data.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            // Comment in data.
            AnalysisContext ident = obj as AnalysisContext;
            if (ident != null)
            {
                /* 
                 Block comment.
                 */
                return SymbolEqualityComparer.Default.Equals(this.TypeSymbol, ident.TypeSymbol) && SymbolEqualityComparer.Default.Equals(this.MethodSymbol, ident.MethodSymbol);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (this.AssemblyPath + (this.MethodSymbol?.Name ?? "") + (this.TypeSymbol?.Name ?? "")).GetHashCode();
        }
    }
}

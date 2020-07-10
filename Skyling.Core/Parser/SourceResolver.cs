using Microsoft.CodeAnalysis;
using Skyling.Core.Decompilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Parser
{
    /// <summary>
    /// Class focused on returning Documents, or fragments of them, for proper analysis. Searches first the primary solution, then all references, 
    /// decompiling when neccessary.
    /// </summary>
    public class SourceResolver
    {
        public SourceResolver(string solutionPath) 
        {
            SolutionResolver.LoadSolutionAndWait(solutionPath);

            ExternalResolver.AddProjectPaths(
                SolutionResolver.Projects.SelectMany(val => val.MetadataReferences));
        }

        public SolutionResolver SolutionResolver { get; private set; } = new SolutionResolver();

        public ExternalLibraryResolver ExternalResolver { get; private set; } = new ExternalLibraryResolver();

        /// <summary>
        /// Gets Document for a particular clas name. Will only work on original solution classes.
        /// </summary>
        public Document GetDocument(AssemblyIdentity assembly, string className) 
        {
            if (assembly == null)
                return null;

            var doc = SolutionResolver.GetDocument(assembly.Name, className);
            if (doc != null)
                return doc;

            return ExternalResolver.GetDocument(assembly, className);
        }

        public Project GetProject(AssemblyIdentity assemIdent)
        {
            if (assemIdent == null)
                return null;

            var proj = SolutionResolver.GetProject(assemIdent.Name);
            if (proj != null)
                return proj;

            return ExternalResolver.GetProject(assemIdent);
        }
    }
}

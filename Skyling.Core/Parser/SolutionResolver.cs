using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skyling.Core.Parser
{
    /// <summary>
    /// Resolves solution
    /// </summary>
    public class SolutionResolver
    {
        static SolutionResolver()
        {
            MSBuildLocator.RegisterDefaults();
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private MSBuildWorkspace workspace;

        public Solution CurrentSolution => workspace.CurrentSolution;

        public IEnumerable<Project> Projects => CurrentSolution.Projects;

        public SolutionResolver()
        {
            workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += (sender, args) => logger.Error($"Couldn't load workspace: {args.Diagnostic.Message}");
        }

        /// <summary>
        /// Constructor that takes a solution file and loads it syncronously.
        /// </summary>
        /// <param name="solutionFile"></param>
        public SolutionResolver(string solutionFile) : this()
        {
            LoadSolutionAndWait(solutionFile);
        }

        /// <summary>
        /// Attempt to load solution and block until it's been fully resolved.
        /// </summary>
        /// <param name="solutionFile"></param>
        public void LoadSolutionAndWait(string solutionFile)
        {
            if (workspace.CurrentSolution.FilePath == solutionFile)
                return;

            var awaitable = workspace.OpenSolutionAsync(solutionFile).ConfigureAwait(true).GetAwaiter();
            while (!awaitable.IsCompleted) { }
        }

        public void LoadProjectAndWait(string projectFile) 
        {
            if (workspace.CurrentSolution.Projects.Any(val => val.FilePath == projectFile))
                return;

            var awaitable = workspace.OpenProjectAsync(projectFile);
            awaitable.RunSynchronously();
            while (!awaitable.IsCompleted) { }
        }

        public Project GetProject(AssemblyIdentity ident) => GetProject(ident.Name);


        public Project GetProject(string assemblyName) => Projects.FirstOrDefault(val => val.AssemblyName == assemblyName);

        public Document GetDocument(string assemblyName, string documentName) 
        {
            foreach (var proj in Projects.Where(val => val.AssemblyName == assemblyName))
            {
                var classDocument = proj.Documents.FirstOrDefault(val => val.Name == documentName);
                if (classDocument != null)
                    return classDocument;
            }

            return null;
        }
    }
}

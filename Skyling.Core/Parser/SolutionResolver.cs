using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NLog;
using System.Collections.Generic;

namespace Skyling.Core.Parser
{
    /// <summary>
    /// Resolves solution
    /// </summary>
    public class SolutionResolver
    {
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
            var awaitable = workspace.OpenSolutionAsync(solutionFile).GetAwaiter();
            while (!awaitable.IsCompleted) { }
        }

        public void LoadProjectAndWait(string projectFIle) 
        {
            var awaitable = workspace.OpenProjectAsync(projectFIle).GetAwaiter();
            while (!awaitable.IsCompleted) { }
        }
    }
}

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using NLog;
using Skyling.Core.Parser.TreeWalkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// Has our solution been resolved yet.
        /// </summary>
        public bool Loaded { get; private set; } = false;

        public SolutionResolver()
        {
            MSBuildLocator.RegisterDefaults();

            workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += (sender, args) => logger.Error($"Couldn't load workspace: {args.Diagnostic.Message}");
        }

        /// <summary>
        /// Constructor that takes a solution file and loads it syncronously.
        /// </summary>
        /// <param name="solutionFile"></param>
        public SolutionResolver(string solutionFile) : this()
        {
            LoadAndWait(solutionFile);
        }

        /// <summary>
        /// Attempt to load solution and block until it's been fully resolved.
        /// </summary>
        /// <param name="solutionFile"></param>
        public void LoadAndWait(string solutionFile)
        {
            LoadSolution(solutionFile);
            while (!Loaded) { }
        }

        /// <summary>
        /// Load the passed-in solution file asynchronously. Sets <see cref="Loaded"/> to True when complete.
        /// </summary>
        /// <param name="solutionFile"></param>
        public async void LoadSolution(string solutionFile)
        {
            await workspace.OpenSolutionAsync(solutionFile);
            Loaded = true;
        }

        public IEnumerable<PotentialTraitsWalker> AnalyzeProjects()
        {
            List<PotentialTraitsWalker> results = new List<PotentialTraitsWalker>();
            foreach (var project in Projects)
            {
                results.AddRange(AnalyzeProject(project));
            }

            return results;
        }

        public IEnumerable<PotentialTraitsWalker> AnalyzeProject(string projectName)
        {
            Project proj = Projects.FirstOrDefault(val => val.Name == projectName);
            if (proj != null)
                return AnalyzeProject(proj);

            return Enumerable.Empty<PotentialTraitsWalker>();
        }

        
        private IEnumerable<PotentialTraitsWalker> AnalyzeProject(Project project)
        {
            List<PotentialTraitsWalker> fileWalkers = new List<PotentialTraitsWalker>();
            foreach (Document doc in project.Documents.Where(val => val.SourceCodeKind == SourceCodeKind.Regular && val.SupportsSyntaxTree && val.SupportsSemanticModel))
            {
                var treeTask = doc.GetSyntaxTreeAsync();
                treeTask.Wait();

                var compilationTask = project.GetCompilationAsync();
                compilationTask.Wait();

                CSharpCompilation compilation = compilationTask.Result as CSharpCompilation;
                if (compilation != null)
                {
                    SyntaxTree tree = treeTask.Result;
                    SyntaxTree blockedTree = tree.WithRootAndOptions(new ExpandReturnsRewriter().Visit(tree.GetRoot()), tree.Options);
                    compilation = compilation.ReplaceSyntaxTree(tree, blockedTree);

                    SemanticModel semanticModel = compilation.GetSemanticModel(blockedTree, true);
                    SyntaxTree rewrittenTree = tree.WithRootAndOptions(new SplitReturnsRewriter(semanticModel).Visit(blockedTree.GetRoot()).NormalizeWhitespace(), tree.Options);
                    compilation = compilation.ReplaceSyntaxTree(blockedTree, rewrittenTree);
                    semanticModel = compilation.GetSemanticModel(rewrittenTree, true);

                    CSharpSyntaxVisitor[] walkers = new CSharpSyntaxVisitor[] { new PotentialTraitsWalker(), new LogicModelWalker(semanticModel) };
                    foreach (CSharpSyntaxVisitor walker in walkers)
                    {
                        walker.Visit(rewrittenTree.GetRoot());
                    }
                }
            }

            return fileWalkers;
        }
    }
}

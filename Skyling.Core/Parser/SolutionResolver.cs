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

        public IEnumerable<CommentsWalker> AnalyzeProjects()
        {
            List<CommentsWalker> results = new List<CommentsWalker>();
            foreach (var project in Projects)
            {
                results.AddRange(AnalyzeProject(project));
            }

            return results;
        }

        public IEnumerable<CommentsWalker> AnalyzeProject(string projectName)
        {
            Project proj = Projects.FirstOrDefault(val => val.Name == projectName);
            if (proj != null)
                return AnalyzeProject(proj);

            return Enumerable.Empty<CommentsWalker>();
        }

        
        private IEnumerable<CommentsWalker> AnalyzeProject(Project project)
        {
            List<CommentsWalker> fileWalkers = new List<CommentsWalker>();
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
                    SemanticModel semanticModel = compilation.GetSemanticModel(tree, true);

                    SyntaxNode root = tree.GetRoot();
                    root.TrackNodes(root.DescendantNodes().OfType<ReturnStatementSyntax>());
                    new ReturnRewriter(semanticModel, root).Visit(root);

                    foreach (CSharpSyntaxVisitor walker in  new CSharpSyntaxVisitor[] { new CommentsWalker(), new ExpressionWalker(semanticModel) })
                    {
                        walker.Visit(root);
                    }

                    //CommentsWalker walker = new CommentsWalker();
                    //walker.Visit(tree.GetRoot());
                    //FileWalker fileWalker = new FileWalker();
                    //fileWalker.File = doc.Name;
                    //fileWalker.Initalize(compilation, this);
                    //fileWalker.Visit(tree.GetRoot());
                    //fileWalkers.Add(fileWalker);
                }
            }

            return fileWalkers;
        }

    }
}

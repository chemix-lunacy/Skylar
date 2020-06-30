using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Skyling.Core.Concepts;
using Skyling.Core.Decompilation;
using Skyling.Core.Parser;
using Skyling.Core.Parser.TreeWalkers;
using System.Collections.Generic;
using System.Linq;

namespace Skyling.Core
{
    public class SkylingEngine
    {
        static SkylingEngine() 
        {
            MSBuildLocator.RegisterDefaults();
        }

        public SkylingEngine(string solutionPath) 
        {
            SolutionResolver.LoadSolutionAndWait(solutionPath);

            DecompilationEngine.AddProjectPaths(
                SolutionResolver.Projects.SelectMany(val => val.MetadataReferences));
        }

        public DecompilationEngine DecompilationEngine { get; set; } = new DecompilationEngine();

        public SolutionResolver SolutionResolver { get; set; } = new SolutionResolver();


        public IEnumerable<PotentialTraitsWalker> AnalyzeProjects()
        {
            List<PotentialTraitsWalker> results = new List<PotentialTraitsWalker>();
            foreach (var project in SolutionResolver.Projects)
            {
                results.AddRange(AnalyzeProject(project));
            }

            return results;
        }

        public IEnumerable<PotentialTraitsWalker> AnalyzeProject(string projectName)
        {
            Project proj = SolutionResolver.Projects.FirstOrDefault(val => val.Name == projectName);
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

                    TraitsStorage traits = new TraitsStorage();
                    CSharpSyntaxVisitor[] walkers = new CSharpSyntaxVisitor[] { new PotentialTraitsWalker(semanticModel, traits, DecompilationEngine), new LogicModelWalker(semanticModel, traits) };
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

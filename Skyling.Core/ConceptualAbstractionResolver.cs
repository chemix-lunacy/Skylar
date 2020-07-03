using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Skyling.Core.Concepts;
using Skyling.Core.Concepts.Walkers;
using Skyling.Core.Logic.Walkers;
using Skyling.Core.Parser;
using Skyling.Core.Parser.Walkers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skyling.Core
{
    /// <summary>
    /// Takes a method and builds a conceptual abstraction for it. This includes a trait set for the method and for all logic models generated.
    /// </summary>
    public class ConceptualAbstractionResolver
    {
        public ConceptualAbstractionResolver(string solutionPath)
        {
            SourceResolver = new SourceResolver(solutionPath);
            TraitResolver = new TraitResolver(this);
        }

        SourceResolver SourceResolver { get; set; }

        SolutionResolver SolutionResolver { get => SourceResolver?.SolutionResolver; }

        TraitResolver TraitResolver { get; set; }

        /// <summary>
        /// Analyzes all projects in the primary Solution. Does not take into account external references.
        /// </summary>
        public void AnalyzeProjects()
        {
            foreach (var project in SolutionResolver.Projects)
            {
                AnalyzeProject(project);
            }
        }

        public void AnalyzeProject(string projectName)
        {
            Project proj = SourceResolver.GetProject(projectName);
            if (proj != null)
                AnalyzeProject(SourceResolver.SolutionResolver.GetProject(projectName));
        }

        public void AnalyzeProject(Project project)
        {
            var compilationTask = project.GetCompilationAsync();
            compilationTask.Wait();

            CSharpCompilation compilation = compilationTask.Result as CSharpCompilation;
            if (compilation == null)
                return;

            foreach (Document doc in project.Documents)
                AnalyzeDocument(compilation, doc);
        }

        public void AnalyzeDocument(CSharpCompilation compilation, Document doc)
        {
            if (doc == null || compilation == null || doc.SourceCodeKind != SourceCodeKind.Regular 
                || !doc.SupportsSyntaxTree || !doc.SupportsSemanticModel)
                return;

            var treeTask = doc.GetSyntaxTreeAsync();
            treeTask.Wait();

            SyntaxTree tree = treeTask.Result;
            SyntaxTree blockedTree = tree.WithRootAndOptions(new ExpandReturnsRewriter().Visit(tree.GetRoot()), tree.Options);
            compilation = compilation.ReplaceSyntaxTree(tree, blockedTree);

            SemanticModel semanticModel = compilation.GetSemanticModel(blockedTree, true);
            SyntaxTree rewrittenTree = tree.WithRootAndOptions(new SplitReturnsRewriter(semanticModel).Visit(blockedTree.GetRoot()).NormalizeWhitespace(), tree.Options);
            compilation = compilation.ReplaceSyntaxTree(blockedTree, rewrittenTree);
            semanticModel = compilation.GetSemanticModel(rewrittenTree, true);

            PotentialTraitsWalker potentialTraits = new PotentialTraitsWalker(semanticModel, TraitResolver);
            potentialTraits.Visit(rewrittenTree.GetRoot());

            LogicModelWalker logicModels = new LogicModelWalker(semanticModel, TraitResolver);
            logicModels.Visit(rewrittenTree.GetRoot());

        }
    }
}

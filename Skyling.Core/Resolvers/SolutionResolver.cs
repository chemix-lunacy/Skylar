using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using NLog;
using Skyling.Core.Parser;
using Skyling.Core.Parser.TreeWalkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skyling.Core.Resolvers
{
    /// <summary>
    /// Resolves solution
    /// </summary>
    public class SolutionResolver
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private MSBuildWorkspace workspace = MSBuildWorkspace.Create();

        public Solution CurrentSolution => this.workspace.CurrentSolution;

        public IEnumerable<Project> Projects => this.CurrentSolution.Projects;

        /// <summary>
        /// Constructor that takes a solution file and loads it for analysis.
        /// </summary>
        /// <param name="solutionFile"></param>
        public SolutionResolver(string solutionFile) 
        {
            workspace.WorkspaceFailed += (sender, args) => Console.WriteLine(args.Diagnostic.Message);
            LoadSolution(solutionFile);
        }

        /// <summary>
        /// Load the passed-in solution file asynchronously.
        /// </summary>
        /// <param name="solutionFile"></param>
        public async void LoadSolution(string solutionFile)
        {
            await workspace.OpenSolutionAsync(solutionFile);
        }

        public TypeWalker AnalyzeType(AnalysisContext identity)
        {
            if (identity.TypeSymbol == null)
                return new TypeWalker() { Incomplete = true };

            TypeWalker result = this.AnalyzeSolutionType(identity);
            //if (result == null)
            //{
            //    result = this.AnalyzeExternalType(identity);
            //}

            return result ?? new TypeWalker() { Incomplete = true };
        }

        public MethodWalker AnalyzeMethod(AnalysisContext identity)
        {
            if (identity.MethodSymbol == null || identity.TypeSymbol == null)
                return new MethodWalker() { Incomplete = true };

            MethodWalker result = this.AnalyzeSolutionMethod(identity);
            //if (result == null)
            //{
            //    result = this.AnalyzeExternalMethod(identity);
            //}

            return result ?? new MethodWalker() { Incomplete = true };
        }

        private Dictionary<AnalysisContext, MethodWalker> methodCache = new Dictionary<AnalysisContext, MethodWalker>();

        //private MethodWalker AnalyzeExternalMethod(AnalysisContext identity)
        //{
        //    if (methodCache.ContainsKey(identity))
        //        return methodCache[identity];

        //    MethodWalker result = null;
        //    DecompiledAssembly decompAssembly = this.assemblyAnalyzer.GetAssembly(identity);
        //    if (decompAssembly != null)
        //    {
        //        ISymbol comparedSymbol = GetMethodSymbol(decompAssembly.Compilation, identity);
        //        if (comparedSymbol != null)
        //        {
        //            MethodWalker methodWalker = new MethodWalker();
        //            methodCache.Add(identity, methodWalker);

        //            methodWalker.Initalize(decompAssembly.Compilation, this);
        //            foreach (SyntaxReference referance in comparedSymbol.DeclaringSyntaxReferences)
        //            {
        //                methodWalker.Visit(referance.GetSyntax());
        //            }
        //            result = methodWalker;
        //        }
        //    }

        //    return result;
        //}

        private MethodWalker AnalyzeSolutionMethod(AnalysisContext identity)
        {
            if (methodCache.ContainsKey(identity))
                return methodCache[identity];

            MethodWalker result = null;
            Project proj = this.Projects.FirstOrDefault(val => val.AssemblyName == identity.MethodSymbol.ContainingAssembly.Name);
            if (proj != null)
            {
                Task<Compilation> compilationTask = proj.GetCompilationAsync();
                compilationTask.Wait();
                CSharpCompilation compilation = compilationTask.Result as CSharpCompilation;

                ISymbol comparedSymbol = GetMethodSymbol(compilation, identity);
                if (comparedSymbol != null)
                {
                    MethodWalker methodWalker = new MethodWalker();
                    methodCache.Add(identity, methodWalker);

                    methodWalker.Initalize(compilation, this);
                    foreach (SyntaxReference referance in comparedSymbol.DeclaringSyntaxReferences)
                    {
                        methodWalker.Visit(referance.GetSyntax());
                    }

                    result = methodWalker;
                }
            }

            return result;
        }

        private ISymbol GetMethodSymbol(CSharpCompilation compilation, AnalysisContext identity)
        {
            List<IMethodSymbol> prospectiveMethods = new List<IMethodSymbol>(compilation.GetSymbolsWithName(name => name == identity.MethodSymbol.Name).OfType<IMethodSymbol>());
            if (!prospectiveMethods.Any())
            {
                INamedTypeSymbol type = this.GetTypeSymbol(compilation, identity) as INamedTypeSymbol;
                if (type != null)
                {
                    prospectiveMethods.AddRange(type.GetMembers().OfType<IMethodSymbol>());
                }
            }

            ISymbol comparedSymbol = prospectiveMethods.FirstOrDefault(val => identity.MethodSymbol.Equals(val));

            /*
             * In some situations (such as reference assemblies) you can't do a straight-up comparison between symbols as 
             * some details will be subtely different between the real / metadata assemblies.
             */
            if (comparedSymbol == null)
            {
                // TODO: This will not deal with generics. Probably. 
                comparedSymbol =
                    prospectiveMethods.FirstOrDefault(symb => symb.MetadataName == identity.MethodSymbol.MetadataName
                    && symb.Parameters.Length == identity.MethodSymbol.Parameters.Length
                    && identity.MethodSymbol.Parameters.All(para => symb.Parameters.Any(val => val.Ordinal == para.Ordinal && para.Name == val.Name && para.Type.Name == val.Type.Name)));
            }

            if (comparedSymbol == null)
            {
                logger.Error($"Attempt to find {identity.MethodSymbol.Name} in {identity.MethodSymbol.ContainingAssembly.Name} has failed.");
            }

            return comparedSymbol;
        }
        
        private Dictionary<AnalysisContext, TypeWalker> typeCache = new Dictionary<AnalysisContext, TypeWalker>();

        //private TypeWalker AnalyzeExternalType(AnalysisContext identity)
        //{
        //    if (typeCache.ContainsKey(identity))
        //        return typeCache[identity];

        //    TypeWalker result = null;
        //    DecompiledAssembly decompAssembly = this.assemblyAnalyzer.GetAssembly(identity);
        //    if (decompAssembly != null)
        //    {
        //        ISymbol typeSymbol = GetTypeSymbol(decompAssembly.Compilation, identity);
        //        if (typeSymbol != null)
        //        {
        //            TypeWalker fileWalker = new TypeWalker();
        //            typeCache.Add(identity, fileWalker);

        //            fileWalker.Initalize(decompAssembly.Compilation, this);
        //            foreach (SyntaxReference referance in typeSymbol.DeclaringSyntaxReferences)
        //            {
        //                fileWalker.Visit(referance.GetSyntax());
        //            }

        //            result = fileWalker;
        //        }
        //    }
            
        //    return result;
        //}

        private TypeWalker AnalyzeSolutionType(AnalysisContext identity)
        {
            if (typeCache.ContainsKey(identity))
                return typeCache[identity];

            TypeWalker result = null;
            Project proj = this.Projects.FirstOrDefault(val => val.AssemblyName == identity.TypeSymbol.ContainingAssembly.Name);
            if (proj != null)
            {
                Task<Compilation> compilationTask = proj.GetCompilationAsync();
                compilationTask.Wait();
                CSharpCompilation compilation = compilationTask.Result as CSharpCompilation;

                ISymbol typeSymbol = GetTypeSymbol(compilation, identity);
                if (typeSymbol != null)
                {
                    TypeWalker fileWalker = new TypeWalker();
                    typeCache.Add(identity, fileWalker);

                    fileWalker.Initalize(compilation, this);
                    foreach (SyntaxReference referance in typeSymbol.DeclaringSyntaxReferences)
                    {
                        fileWalker.Visit(referance.GetSyntax());
                    }

                    result = fileWalker;
                }
            }

            return result;
        }

        private INamedTypeSymbol GetTypeSymbol(CSharpCompilation compilation, AnalysisContext identity)
        {
            IEnumerable<INamedTypeSymbol> prospectiveTypes = compilation.GetSymbolsWithName(name => name == identity.TypeSymbol.Name).OfType<INamedTypeSymbol>();
            if (!prospectiveTypes.Any())
            {
                prospectiveTypes = new INamedTypeSymbol[] { compilation.GetTypeByMetadataName(identity.TypeSymbol.MetadataName) };
            }

            return prospectiveTypes.FirstOrDefault();
        }

        public IEnumerable<FileWalker> AnalyzeProjects()
        {
            List<FileWalker> results = new List<FileWalker>();
            foreach (var project in this.Projects)
            {
                results.AddRange(this.AnalyzeProject(project));
            }

            return results;
        }

        public IEnumerable<FileWalker> AnalyzeProject(string projectName)
        {
            Project proj = this.Projects.FirstOrDefault(val => val.Name == projectName);
            if (proj != null)
                return this.AnalyzeProject(proj);

            return Enumerable.Empty<FileWalker>();
        }

        private IEnumerable<FileWalker> AnalyzeProject(Project project)
        {
            List<FileWalker> fileWalkers = new List<FileWalker>();
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
                    FileWalker fileWalker = new FileWalker();
                    fileWalker.File = doc.Name;
                    fileWalker.Initalize(compilation, this);
                    fileWalker.Visit(tree.GetRoot());
                    fileWalkers.Add(fileWalker);
                }
            }

            return fileWalkers;
        }

    }
}

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Skyling.Core.Decompilation
{
    public class AssemblyAnalyzer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<string, DecompiledAssembly> loadedModules = new Dictionary<string, DecompiledAssembly>();

        public DecompiledAssembly GetAssembly(AnalysisContext identity)
        {
            string assemblyPath = identity.AssemblyPath;

            // Reference assemblies only hold metadata, the actual assemblies live elsewhere.
            if (assemblyPath.Contains("Reference Assemblies"))
            {
                try
                {
                    string assemblyName = identity.MethodSymbol?.ContainingAssembly.Name ?? identity.TypeSymbol?.ContainingAssembly.Name ?? "";
                    Assembly assem = Assembly.Load(assemblyName);
                    assemblyPath = assem?.Location ?? string.Empty;
                }
                catch
                {
                }
            }

            DecompiledAssembly decompAssembly;
            if (!loadedModules.TryGetValue(assemblyPath, out decompAssembly))
            {
                try
                {
                    ModuleDefinition moduleDef = ModuleDefinition.ReadModule(assemblyPath);

                    Dictionary<string, SyntaxTree> syntaxTrees = new Dictionary<string, SyntaxTree>();
                    IEnumerable<TypeDefinition> typeDefs = moduleDef.GetTypes();
                    foreach (TypeDefinition typeDef in typeDefs)
                    {
                        DecompilerContext context = new DecompilerContext(moduleDef);
                        AstBuilder astBuilder = new AstBuilder(context);
                        astBuilder.AddType(typeDef);

                        PlainTextOutput textOutput = new PlainTextOutput();
                        astBuilder.GenerateCode(textOutput);
                        string codeText = textOutput.ToString();

                        syntaxTrees.Add(typeDef.FullName, SyntaxFactory.ParseSyntaxTree(codeText));
                    }
                    
                    // This compilation is nowhere near what you'd get if you picked it from a project file. For one it's lacking version and key information from the strong name.
                    // TODO: If possible, recreate identifying information from the decopiled assembly.
                    CSharpCompilation compilation = CSharpCompilation.Create(moduleDef?.Assembly.Name.Name ?? moduleDef.FullyQualifiedName, syntaxTrees.Values, new[] { MetadataReference.CreateFromFile(assemblyPath) });

                    decompAssembly = new DecompiledAssembly() { Path = assemblyPath, Compilation = compilation, ModuleDef = moduleDef, Assembly = Assembly.LoadFile(assemblyPath), SyntaxTrees = syntaxTrees };
                    loadedModules.Add(assemblyPath, decompAssembly);
                }
                catch (Exception ex)
                {
                    logger.Error($"Decompilation attempt failed. Message: {ex.Message}. Inner message: {ex.InnerException?.Message}");
                }
            }

            return decompAssembly;
        }
    }
}

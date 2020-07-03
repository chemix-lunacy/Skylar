using Microsoft.CodeAnalysis;
using Skyling.Core.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Decompilation
{
    public class ExternalLibraryResolver
    {
		public SolutionResolver ProjectResolver { get; private set; } = new SolutionResolver();

		public List<AssemblyPathData> AssemblyPaths { get; set; } = new List<AssemblyPathData>();

		public DecompilationEngine DecompilationEngine { get; private set; } = new DecompilationEngine();

		public void AddProjectPaths(IEnumerable<MetadataReference> references)
		{
			// We don't want our main AppDomain poluted as all we really care about are the paths.
			AppDomain dom = AppDomain.CreateDomain("temp");
			AssemblyPaths.AddRange(
				references.Select(val =>
				{
					try
					{
						// Need to load directly as we have some reference assemblies, so we want to know what the actual dll location is.
						AssemblyName assemblyName = AssemblyName.GetAssemblyName(val.Display);
						Assembly assembly = dom.Load(assemblyName);
						return new AssemblyPathData { Identity = AssemblyIdentity.FromAssemblyDefinition(assembly), Location = assembly.Location };
					}
					catch (Exception ex)
					{
						return null;
					}
				})
				.Where(val => val != null)
				.Distinct());
		}

		public Project GetProject(string projectName)
		{
			if (string.IsNullOrEmpty(projectName))
				return null;

			AssemblyPathData data = AssemblyPaths.FirstOrDefault(val => val.Identity.Name == projectName);
			if (data != null)
			{
				string projectPath = DecompilationEngine.DecompileProject(data);
				string projectFile = Path.Combine(projectPath, $"{Path.GetFileNameWithoutExtension(data.Location)}.csproj");
				ProjectResolver.LoadProjectAndWait(projectFile);
				return ProjectResolver.GetProject(projectName);
			}

			return null;
		}

		public Document GetDocument(AssemblyIdentity identity, string className)
		{
			if (identity == null || string.IsNullOrEmpty(className))
				return null;

			AssemblyPathData data = AssemblyPaths.FirstOrDefault(val => val.Identity == identity);
			if (data != null) 
			{
				string projectPath = DecompilationEngine.DecompileProject(data);
				string projectFile = Path.Combine(projectPath, $"{Path.GetFileNameWithoutExtension(data.Location)}.csproj");
				ProjectResolver.LoadProjectAndWait(projectFile);
				return ProjectResolver.GetDocument(identity.Name, className);
			}

			return null;
		}

		/// <summary>
		/// Decompiles and indexes the project this symbol is a part of (if viable), returns the Project object this symbol is a part of.
		/// </summary>
		public Document GetDocument(ISymbol symbol)
		{
			if (symbol?.ContainingAssembly == null)
				return null;

			return GetDocument(symbol.ContainingAssembly.Identity, symbol.ContainingType?.Name);
		}
	}
}

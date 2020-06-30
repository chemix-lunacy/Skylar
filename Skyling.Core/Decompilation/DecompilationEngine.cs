using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.CodeAnalysis;
using Skyling.Core.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Skyling.Core.Decompilation
{
	public class DecompilationEngine
    {
		public class AssemblyPathData 
		{ 
			public string Location { get; set; }

			public AssemblyIdentity Identity { get; set; }

			public override bool Equals(object obj)
			{
				return obj is AssemblyPathData data ? data.Location == Location : base.Equals(obj);
			}

			public override int GetHashCode()
			{
				return Location != null ? Location.GetHashCode() : base.GetHashCode();
			}
		}

		public List<AssemblyPathData> AssemblyPaths { get; set; } = new List<AssemblyPathData>();

		public string OutputDirectory { get; set; } = @"D:\Source\Decompiled Projects";

		public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Latest;

		public IEnumerable<string> ReferencePaths { get; set; } = new List<string>();

		public SolutionResolver ProjectResolver = new SolutionResolver();

		public void AddProjectPaths(IEnumerable<MetadataReference> references) 
		{
			// We don't want our main AppDomain poluted as all we really care about are the actual 
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

		public void ResolveAssembly(IAssemblySymbol symbol)
		{
			AssemblyPathData data = AssemblyPaths.FirstOrDefault(val => val.Identity == symbol.Identity);
			if (data != null)
			{
				string projectPath = DecompileProject(data);
				string projectFile = Path.Combine(projectPath, $"{Path.GetFileNameWithoutExtension(data.Location)}.csproj");
				ProjectResolver.LoadProjectAndWait(projectFile);
			}
		}
			 
		public string DecompileProject(AssemblyPathData pathData)
		{
			string projectPath = Path.Combine(OutputDirectory, $"{Path.GetFileNameWithoutExtension(pathData.Location)}-{pathData.Identity.Version}");
			if (Directory.Exists(projectPath))
				return projectPath;

			Directory.CreateDirectory(projectPath);
			using (var module = new PEFile(pathData.Location))
			{
				var resolver = new UniversalAssemblyResolver(pathData.Location, false, module.Reader.DetectTargetFrameworkId());
				foreach (var path in ReferencePaths)
				{
					resolver.AddSearchDirectory(path);
				}

				var decompiler = new WholeProjectDecompiler()
				{
					AssemblyResolver = resolver
				};

				decompiler.DecompileProject(module, projectPath);
			}

			return projectPath;
		}
	}
}

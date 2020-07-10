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
		public IEnumerable<string> ReferencePaths { get; set; } = new List<string>();

		// TODO: Remove hard-coded path.
		public string OutputDirectory { get; set; } = @"D:\Source\Decompiled Projects";

		public string DecompileAndGetProjectFile(AssemblyPathData data)
		{
			string projectPath = DecompileProject(data);
			return Path.Combine(projectPath, $"{Path.GetFileNameWithoutExtension(data.Location)}.csproj");
		}
			 
		/// <summary>
		/// Decompiles the project linked to the assembly path data and returns folder where the project has been decompiled.
		/// </summary>
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

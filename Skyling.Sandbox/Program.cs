using System.IO;
using System.Reflection;
using Skyling.Core.Parser;
using Skyling.Core.Resolvers;

namespace Skyling.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            string solutionPath = FindSkylingPath();
            SolutionResolver solution = new SolutionResolver(solutionPath);
            LogicParser parser = new LogicParser(solution);
            parser.GenerateLogicModel();
        }

        static string FindSkylingPath()
        {
            return FindSkylingPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
        }

        static string FindSkylingPath(string root, int depth = 0)
        {
            DirectoryInfo dir = Directory.GetParent(root);
            string skylingPath = Path.Combine(root, "Skyling.sln");
            if (File.Exists(skylingPath))
                return skylingPath;
            else if (depth >= 7)
                return "";
            else
                return FindSkylingPath(dir.Parent.FullName, ++depth);
        }
    }
}
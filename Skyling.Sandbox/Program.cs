using System.IO;
using System.Reflection;
using Skyling.Core.Parser;
using Skyling.Core.Resolvers;
using Skyling.ML;

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

            BasicBinary ta = new BasicBinary();
            ta.Train();

            MultiClassification mc = new MultiClassification();
            mc.Train();
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
            else if (depth >= 7 || dir.Parent == null)
                return "";
            else
                return FindSkylingPath(dir.FullName, ++depth);
        }
    }
}
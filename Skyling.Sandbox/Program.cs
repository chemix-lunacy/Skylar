using System.IO;
using System.Reflection;
using Skyling.Core.Parser;
using Skyling.ML;
using Skyling.Sandbox.Examples;

namespace Skyling.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            // Testing commit.
            string solutionPath = FindSkylingPath();
            SolutionResolver solution = new SolutionResolver(solutionPath);
            LogicParser parser = new LogicParser(solution);
            parser.GenerateLogicModel("Skyling.Sandbox");

            StringCombining comb = new StringCombining();
            string resultOne = comb.CombineOne("a", "b", "c");
            string resultTwo = comb.CombineOne("d", "e", "f");
            string resultThree = comb.CombineOne("g", "h", "i");

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
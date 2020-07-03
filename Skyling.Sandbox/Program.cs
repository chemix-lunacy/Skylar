using System.IO;
using System.Reflection;
using Skyling.Core;
using Skyling.Core.Logic;
using Skyling.Sandbox.Examples;

namespace Skyling.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            // Testing commit.
            string solutionPath = FindSkylingPath();
            ConceptualAbstractionResolver solution = new ConceptualAbstractionResolver(solutionPath);
            solution.AnalyzeProject("Skyling.Sandbox");

            StringCombining comb = new StringCombining();
            string resultOne = comb.FormatNameAddress("a", "b", "c");
            string resultTwo = comb.FormatNameAddress("d", "e", "f");
            string resultThree = comb.FormatNameAddress("g", "h", "i");

            //BasicBinary ta = new BasicBinary();
            //ta.Train();

            //MultiClassification mc = new MultiClassification();
            //mc.Train();
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
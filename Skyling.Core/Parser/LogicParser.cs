using Skyling.Core.Logic;
using Skyling.Core.Parser.TreeWalkers;
using System.Collections.Generic;
using System.Linq;

namespace Skyling.Core.Parser
{
    public class LogicParser
    {
        private SkylingEngine solutionResolver;

        public LogicParser(SkylingEngine resolver)
        {
            this.solutionResolver = resolver;
        }

        public IEnumerable<LogicModel> GenerateLogicModel(string projectName)
        {
            List<PotentialTraitsWalker> parserOutput = new List<PotentialTraitsWalker>();
            parserOutput.AddRange(this.solutionResolver.AnalyzeProject(projectName));

            return Enumerable.Empty<LogicModel>();
        }

        public IEnumerable<LogicModel> GenerateLogicModel()
        {
            List<PotentialTraitsWalker> parserOutput = new List<PotentialTraitsWalker>();
            parserOutput.AddRange(this.solutionResolver.AnalyzeProjects());

            return Enumerable.Empty<LogicModel>();
        }
    }
}

using Skyling.Core.Parser.TreeWalkers;
using Skyling.Core.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skyling.Core.Parser
{
    public class LogicParser
    {
        private SolutionResolver solutionResolver;

        public LogicParser(SolutionResolver resolver)
        {
            this.solutionResolver = resolver;
        }

        public IEnumerable<FunctionalModel> GenerateLogicModel()
        {
            List<FileWalker> parserOutput = new List<FileWalker>();
            parserOutput.AddRange(this.solutionResolver.AnalyzeProjects());

            return Enumerable.Empty<FunctionalModel>();
        }
    }
}

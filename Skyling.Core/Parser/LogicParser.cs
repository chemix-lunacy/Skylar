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

        public IEnumerable<LogicModel> GenerateLogicModel()
        {
            List<CommentsWalker> parserOutput = new List<CommentsWalker>();
            parserOutput.AddRange(this.solutionResolver.AnalyzeProjects());

            return Enumerable.Empty<LogicModel>();
        }
    }
}

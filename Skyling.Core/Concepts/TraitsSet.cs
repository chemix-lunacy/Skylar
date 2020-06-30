using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Skyling.Core.Concepts
{
    [DebuggerDisplay("[{DebuggerValue,nq}]")]
    public class TraitsSet
    {
        public TraitsSet(params string[] vals) => Traits.UnionWith(vals);

        public TraitsSet(IEnumerable<string> vals) => Traits.UnionWith(vals);

        public TraitsSet(TraitsSet vals)
        {
            if (vals != null)
                Traits.UnionWith(vals.Traits);
        }

        public HashSet<string> Traits { get; set; } = new HashSet<string>();

        public void Add(string val) => Traits.Add(val);

        public TraitsSet Union(TraitsSet set)
        {
            if (set != null)
                return new TraitsSet(set.Traits.Union(this.Traits));
            else
                return new TraitsSet();
        }

        public void UnionWith(TraitsSet set)
        {
            if (set != null)
                Traits.UnionWith(set.Traits);
        }

        private string DebuggerValue => !this.Traits.Any() ? "" : this.Traits.Take(50).Select(val => val).Aggregate((longest, next) => longest + ", " + next) + (this.Traits.Count > 50 ? "..." : "");
    }
}

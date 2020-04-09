using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Concepts
{
    [DebuggerDisplay("[{DebuggerValue,nq}]")]
    public class TraitCollection
    {
        public TraitCollection(params string[] vals) => Traits.AddRange(vals.Select(val => new Trait(val)));

        public TraitCollection(IEnumerable<string> vals) => Traits.AddRange(vals.Select(val => new Trait(val)));

        List<Trait> Traits { get; set; } = new List<Trait>();
        
        private string DebuggerValue => !this.Traits.Any() ? "" : this.Traits.Select(val => val.Value).Aggregate((longest, next) => longest + ", " + next);
    }
}

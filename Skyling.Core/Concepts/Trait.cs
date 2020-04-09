using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Core.Concepts
{
    public class Trait
    {
        public Trait(string val) => this.Value = val;

        public string Value { get; set; }
    }
}

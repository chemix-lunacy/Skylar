using Skyling.Core.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Sandbox.Examples
{
    public class StringCombining
    {
        public int val { get; set; } = 9;

        [Trait("string", "append")]
        [return: Trait("key")]
        public string CombineOne([Trait("address")] string one, [Trait("name")]string two, string three) 
        {
            one = "dave";
            string four = CombineTwo(one, two, three);
            val = val + 1;
            return one + two + three + four;
        }

        [Trait("string", "append")]
        public string CombineTwo(string one, string two, string three)
        {
            val = 2;
            StringBuilder sb = new StringBuilder();
            sb.Append(one);
            sb.Append(two);
            sb.Append(three);
            return sb.ToString();
        }

        [Trait("string", "append")]
        public string CombineThree(string one, string two, string three)
        {
            return $"{one}{two}{three}";
        }
    }
}

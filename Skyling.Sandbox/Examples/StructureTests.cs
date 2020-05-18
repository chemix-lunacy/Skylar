using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyling.Sandbox
{
    public class StructureTests
    {
        private string Property { get; set; }

        private static void StaticMethod() { }

        public void CheckVoidReturn()
        {
            if (true)
                return;
        }

        public void CheckPropertyAssign() 
        {
            Property = "dave";
            string val = Property;
            Property = val;
        }

        public void CallStatic() 
        {
            StaticMethod();
            string nameOfMethod = nameof(StaticMethod);
        }
    }
}

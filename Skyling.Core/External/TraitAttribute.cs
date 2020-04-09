using System;

namespace Skyling.Core.External
{
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class TraitAttribute : Attribute
    {
        public TraitAttribute(params string[] traits)
        {
            this.traits = traits;
        }

        private string[] traits;
    }
}

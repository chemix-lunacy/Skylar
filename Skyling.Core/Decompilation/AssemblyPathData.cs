using Microsoft.CodeAnalysis;

namespace Skyling.Core.Decompilation
{
	public class AssemblyPathData
	{
		public string Location { get; set; }

		public AssemblyIdentity Identity { get; set; }

		public override bool Equals(object obj)
		{
			return obj is AssemblyPathData data ? data.Location == Location : base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Location != null ? Location.GetHashCode() : base.GetHashCode();
		}
	}
}

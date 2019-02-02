namespace Sandbox {
	using Sandbox.Core;
	using Unity.Mathematics;

	public class Interact : Block.Verb { }

	public class Push : Block.Verb {
		public ushort blockID;
		public int3 normal;
	}

	public class Pull : Block.Verb { }
}

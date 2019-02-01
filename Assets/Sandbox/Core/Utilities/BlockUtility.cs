namespace Sandbox.Core {
	using Unity.Mathematics;
	using static Unity.Mathematics.math;

	public static class BlockUtility {
		public static int3[] Normals = new [] {
			int3( 0,  0,  0),
			int3( 0, -1,  0),
			int3( 0,  1,  0),
			int3( 0,  0, -1),
			int3( 0,  0,  1),
			int3(-1,  0,  0),
			int3( 1,  0,  0)
		};
	}
}
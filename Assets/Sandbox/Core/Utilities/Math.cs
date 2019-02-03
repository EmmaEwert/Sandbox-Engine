namespace Sandbox.Core {
	using Unity.Mathematics;

	public static class Math {
		///<summary>Angles in degrees, converted into radians for quat.Euler. TODO: Don't.</summary>
		public static float3 RotateAroundPivot(float3 point, float3 pivot, float3 angles) {
			return math.mul(quaternion.Euler(2f * (float)math.PI * angles / 360f), (point - pivot)) + pivot;
		}
	}

	public static class mathExtension {
		public static int3 down (this int3 pos) => pos + new int3( 0, -1,  0);
		public static int3 up   (this int3 pos) => pos + new int3( 0,  1,  0);
		public static int3 south(this int3 pos) => pos + new int3( 0,  0, -1);
		public static int3 north(this int3 pos) => pos + new int3( 0,  0,  1);
		public static int3 west (this int3 pos) => pos + new int3(-1,  0,  0);
		public static int3 east (this int3 pos) => pos + new int3( 1,  0,  0);
	}
}
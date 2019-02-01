namespace Sandbox.Core {
	using Unity.Mathematics;

	public static class Math {
		public static float3 RotateAroundPivot(float3 point, float3 pivot, float3 angles) {
			return math.mul(quaternion.Euler(angles), (point - pivot)) + pivot;
		}
	}
}
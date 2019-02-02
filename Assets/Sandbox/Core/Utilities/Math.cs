namespace Sandbox.Core {
	using Unity.Mathematics;

	public static class Math {
		///<summary>Angles in radians?</summary>
		public static float3 RotateAroundPivot(float3 point, float3 pivot, float3 angles) {
			return math.mul(quaternion.Euler(2f * (float)math.PI * angles / 360f), (point - pivot)) + pivot;
		}
	}
}
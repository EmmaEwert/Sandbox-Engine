namespace Sandbox.Core {
	using Unity.Mathematics;

	public struct Box {
		public float3 min;
		public float3 max;
		public float3 center => min + halfsize;
		public float3 halfsize => (max - min) / 2f;
	}
}
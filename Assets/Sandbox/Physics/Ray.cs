namespace Sandbox {
	using Unity.Mathematics;

	public struct Ray {
		public float3 origin;
		public float3 direction;
		public float3 inverseDirection { get; private set; }
		public Ray(float3 origin, float3 direction) {
			this.origin = origin;
			this.direction = direction;
			this.inverseDirection = 1f / direction;
		}
	}
}
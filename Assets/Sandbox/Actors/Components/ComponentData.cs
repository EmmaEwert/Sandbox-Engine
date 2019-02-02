namespace Sandbox {
	using Unity.Entities;
	using Unity.Mathematics;

	public struct ActorType : IComponentData {
		public Actor.Type Value;
	}

	public struct Collision : IComponentData {
		public int3 Position;
		public float3 Normal;
	}

	public struct FallDown : IComponentData {
		public byte Grounded;
	}

	public struct RabbitSpawner : IComponentData {
		public float Cooldown;
	}

	public struct RandomMove : IComponentData {
		public float Cooldown;
		public uint State;
	}

	public struct Velocity : IComponentData {
		public float3 Value;
	}

	// Tag components.
	public struct Collider : IComponentData { }
	public struct JumpForward : IComponentData { }

}
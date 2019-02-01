namespace Sandbox {
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Transforms;

	class CollisionDetectionSystem : JobComponentSystem {
		[RequireSubtractiveComponent(typeof(Collision))]
		struct CollisionJob : IJobProcessComponentDataWithEntity<Collider, Position, Velocity> {
			[ReadOnly] public float Δt;
			[ReadOnly] public ushort volumeID;
			[NativeDisableParallelForRestriction] public EntityCommandBuffer commands;

			public void Execute(Entity entity, int index, ref Collider collider, ref Position pos, ref Velocity vel) {
				var box = new Box {
					min = pos.Value - new float3(0.5f) + vel.Value * Δt,
					max = pos.Value + new float3(0.5f) + vel.Value * Δt
				};
				var volume = GameServer.world.volumes[volumeID];
				if (Physics.Intersects(volume, box)) {
					//var manager = Unity.Entities.World.Active.GetOrCreateManager<EntityManager>();
					//manager.AddComponent(entity, typeof(Collision));
					commands.AddComponent(entity, new Collision { });
				}
			}
		}

		[Inject] EndFrameBarrier barrier;

		protected override JobHandle OnUpdate(JobHandle dependencies) {
			return new CollisionJob {
				Δt = UnityEngine.Time.deltaTime,
				volumeID = 0,
				commands = barrier.CreateCommandBuffer()
			}.Schedule(this, dependencies);
		}
	}
}
namespace Sandbox {
	using Sandbox.Core;
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
			[NativeDisableParallelForRestriction] public EntityCommandBuffer queue;

			public void Execute(Entity entity, int index, ref Collider collider, ref Position pos, ref Velocity vel) {
				var box = new Box {
					min = pos.Value - new float3(0.25f) + vel.Value * Δt,
					max = pos.Value + new float3(0.25f) + vel.Value * Δt
				};
				var volume = Server.universe.volumes[volumeID];
				if (Physics.Intersects(volume, box)) {
					queue.AddComponent(entity, new Collision { });
				}
			}
		}

#pragma warning disable 649 // never assigned
		[Inject] EndFrameBarrier barrier;
#pragma warning restore 649

		protected override JobHandle OnUpdate(JobHandle dependencies) {
			return new CollisionJob {
				Δt = UnityEngine.Time.deltaTime,
				volumeID = 0,
				queue = barrier.CreateCommandBuffer()
			}.Schedule(this, dependencies);
		}
	}
}
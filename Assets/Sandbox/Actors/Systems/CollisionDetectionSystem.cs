namespace Sandbox {
	using Sandbox.Core;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Transforms;

	class CollisionDetectionSystem : JobComponentSystem {
		[RequireComponentTag(typeof(Collider))]
		[RequireSubtractiveComponent(typeof(Collision))]
		struct DetectionJob : IJobProcessComponentDataWithEntity<Position, Velocity> {
			[ReadOnly] public float Δt;
			[ReadOnly] public ushort volumeID;
			[NativeDisableParallelForRestriction] public EntityCommandBuffer queue;

			public void Execute([ReadOnly] Entity entity, [ReadOnly] int index, [ReadOnly] ref Position pos, [ReadOnly] ref Velocity vel) {
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
			return new DetectionJob {
				Δt = UnityEngine.Time.deltaTime,
				volumeID = 0,
				queue = barrier.CreateCommandBuffer()
			}.Schedule(this, dependencies);
		}
	}
}
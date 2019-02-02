namespace Sandbox {
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Transforms;

	[UpdateAfter(typeof(GravitySystem))]
	class CollisionResponseSystem : JobComponentSystem {
		[RequireComponentTag(typeof(Collision))]
		struct ResponseJob : IJobProcessComponentDataWithEntity<Position, Velocity> {
			[ReadOnly] public float Δt;
			[NativeDisableParallelForRestriction] public EntityCommandBuffer queue;

			public void Execute([ReadOnly] Entity entity, [ReadOnly] int index, ref Position pos, ref Velocity vel) {
				pos.Value -= vel.Value * Δt;
				pos.Value.y = math.round(pos.Value.y + 0.75f) - 0.75f;
				vel.Value = new float3(0);
				queue.RemoveComponent<Collision>(entity);
			}
		}

#pragma warning disable 649 // never assigned
		[Inject] EndFrameBarrier barrier;
#pragma warning restore 649

		protected override JobHandle OnUpdate(JobHandle dependencies) {
			return new ResponseJob {
				Δt = UnityEngine.Time.deltaTime,
				queue = barrier.CreateCommandBuffer()
			}.Schedule(this, dependencies);
		}
	}
}
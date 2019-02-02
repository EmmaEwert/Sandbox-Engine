namespace Sandbox {
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Transforms;

	[UpdateAfter(typeof(GravitySystem))]
	class CollisionResponseSystem : JobComponentSystem {
		struct ResponseJob : IJobProcessComponentDataWithEntity<Position, Velocity, Collision> {
			public float Δt;
			[NativeDisableParallelForRestriction] public EntityCommandBuffer commands;

			public void Execute(Entity entity, int index, ref Position pos, ref Velocity vel, ref Collision collision) {
				pos.Value -= vel.Value * Δt;
				pos.Value.y = math.round(pos.Value.y + 0.75f) - 0.75f;
				vel.Value = new float3(0);
				commands.RemoveComponent<Collision>(entity);
			}
		}

#pragma warning disable 649 // never assigned
		[Inject] EndFrameBarrier barrier;
#pragma warning restore 649

		protected override JobHandle OnUpdate(JobHandle dependencies) {
			return new ResponseJob {
				Δt = UnityEngine.Time.deltaTime,
				commands = barrier.CreateCommandBuffer()
			}.Schedule(this, dependencies);
		}
	}
}
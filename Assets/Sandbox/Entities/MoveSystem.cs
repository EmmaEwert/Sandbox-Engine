namespace Sandbox {
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Transforms;

	[UpdateAfter(typeof(CollisionResponseSystem))]
	public class MoveSystem : JobComponentSystem {
		[BurstCompile]
		struct MoveJob : IJobProcessComponentData<Position, Velocity> {
			[ReadOnly] public float Δt;

			public void Execute(ref Position pos, [ReadOnly] ref Velocity vel) {
				pos.Value += vel.Value * Δt;
			}
		}

		protected override JobHandle OnUpdate(JobHandle dependencies) {
			return new MoveJob {
				Δt = UnityEngine.Time.deltaTime
			}.Schedule(this, dependencies);
		}
	}
}
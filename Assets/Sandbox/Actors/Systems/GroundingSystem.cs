namespace Sandbox {
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;

	[UpdateBefore(typeof(CollisionResponseSystem))]
	class GroundingSystem : JobComponentSystem {
		[BurstCompile]
		[RequireComponentTag(typeof(Collision))]
		struct GroundingJob : IJobProcessComponentData<FallDown> {
			public void Execute([WriteOnly] ref FallDown fallDown) {
				fallDown.Grounded = 1;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps) {
			return new GroundingJob().Schedule(this, inputDeps);
		}
	}
}
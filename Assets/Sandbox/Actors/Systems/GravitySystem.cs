namespace Sandbox {
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;

	class GravitySystem : JobComponentSystem {
		[BurstCompile]
		[RequireSubtractiveComponent(typeof(Collision))]
		struct GravityJob : IJobProcessComponentData<Velocity, FallDown> {
			[ReadOnly] public float Δt;

			public void Execute(ref Velocity vel, [ReadOnly] ref FallDown fallDown) {
				if (fallDown.Grounded == 1) {
					return;
				}
				vel.Value.y += -20f * Δt;
			}
		}

		protected override JobHandle OnUpdate(JobHandle dependencies) {
			return new GravityJob {
				Δt = UnityEngine.Time.deltaTime
			}.Schedule(this, dependencies);
		}
	}
}
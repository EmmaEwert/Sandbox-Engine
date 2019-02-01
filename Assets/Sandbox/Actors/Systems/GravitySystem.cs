namespace Sandbox {
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Transforms;

	class GravitySystem : JobComponentSystem {
		[BurstCompile]
		[RequireSubtractiveComponent(typeof(Collision))]
		struct GravityJob : IJobProcessComponentData<Position, Velocity, FallDown> {
			public float Δt;

			public void Execute(ref Position pos, ref Velocity vel, ref FallDown fallDown) {
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
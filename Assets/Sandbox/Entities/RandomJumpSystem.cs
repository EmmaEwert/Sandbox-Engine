namespace Sandbox {
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Transforms;

	[UpdateBefore(typeof(MoveSystem))]
	[UpdateAfter(typeof(GravitySystem))]
	class RandomJumpSystem : JobComponentSystem {
		static Random random = new Random(1);

		[BurstCompile]
		[RequireComponentTag(typeof(JumpForward))]
		struct RandomJumpJob : IJobProcessComponentData<RandomMove, Position, Velocity, FallDown> {
			public float Δt;
			public Random random;

			public void Execute(ref RandomMove randomMove, [ReadOnly] ref Position pos, ref Velocity vel, ref FallDown fallDown) {
				//if (pos.Value.y > 0) { return; }
				if (fallDown.Grounded == 0) { return; }
				randomMove.Cooldown -= Δt;
				if (randomMove.Cooldown <= 0f) {
					random.state = randomMove.State;
					randomMove.Cooldown = random.NextFloat() * 4f;
					var direction = random.NextFloat2Direction();
					vel.Value = new float3(direction.x, 4f + 12f * random.NextFloat(), direction.y);
					randomMove.State = random.state;
					fallDown.Grounded = 0;
				}
			}
		}

		protected override JobHandle OnUpdate(JobHandle dependencies) {
			return new RandomJumpJob {
				Δt = UnityEngine.Time.deltaTime,
				random = random
			}.Schedule(this, dependencies);
		}
	}
}
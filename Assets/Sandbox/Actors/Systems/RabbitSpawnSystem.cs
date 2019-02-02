namespace Sandbox {
	using Sandbox.Core;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Transforms;

	class RabbitSpawnSystem : JobComponentSystem {
		static Random random = new Random(1);

		struct RabbitSpawnJob : IJobProcessComponentData<Position, RabbitSpawner> {
			[ReadOnly] public float Δt;
			[ReadOnly] public EntityArchetype rabbit;
			public Random random;
			[NativeDisableParallelForRestriction] public EntityCommandBuffer queue;

			public void Execute([ReadOnly] ref Position pos, ref RabbitSpawner spawner) {
				spawner.Cooldown -= Δt;
				if (spawner.Cooldown < 0f) {
					spawner.Cooldown = 10f;
					queue.CreateEntity(rabbit);
					queue.SetComponent(new ActorType { Value = Actor.Type.Rabbit });
					queue.SetComponent(new Position { Value = pos.Value + new int3(0, 3, 0) });
					queue.SetComponent(new RandomMove { State = random.NextUInt() });
				}
			}
		}

#pragma warning disable 649
		[Inject] EndFrameBarrier barrier;
#pragma warning restore 649

		protected override JobHandle OnUpdate(JobHandle dependencies) {
			if (!Server.running) { return dependencies; }

			return new RabbitSpawnJob {
				Δt = UnityEngine.Time.deltaTime,
				queue = barrier.CreateCommandBuffer(),
				rabbit = Bootstrap.rabbitArchetype,
				random = random
			}.Schedule(this, dependencies);
		}
	}
}
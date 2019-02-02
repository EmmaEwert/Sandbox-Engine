namespace Sandbox {
	using Sandbox.Core;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Transforms;

	class RabbitSpawnSystem : JobComponentSystem {
		static Random random = new Random(1);

		struct SpawnRabbitJob : IJobProcessComponentData<Position, RabbitSpawner> {
			[ReadOnly] public float Δt;
			[NativeDisableParallelForRestriction] public EntityCommandBuffer queue;

			public void Execute([ReadOnly] ref Position pos, ref RabbitSpawner spawner) {
				spawner.Cooldown -= Δt;
				if (spawner.Cooldown < 0f) {
					spawner.Cooldown = 10f;
					queue.CreateEntity(Bootstrap.rabbitArchetype);
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

			return new SpawnRabbitJob {
				Δt = UnityEngine.Time.deltaTime,
				queue = barrier.CreateCommandBuffer()
			}.Schedule(this, dependencies);
		}
	}
}
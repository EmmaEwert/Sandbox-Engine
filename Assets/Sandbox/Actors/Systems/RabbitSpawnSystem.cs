namespace Sandbox {
	using Unity.Entities;
	using Unity.Mathematics;
	using Unity.Transforms;

	class RabbitSpawnSystem : ComponentSystem {
		static float cooldown = 2.5f;
		static Random random = new Random(1);

		protected override void OnUpdate() {
			if (!GameServer.running) { return; }

			cooldown -= UnityEngine.Time.deltaTime;
			if (cooldown < 0f) {
				SpawnRabbit();
				cooldown = 10f;
			}
		}

		void SpawnRabbit() {
			var manager = Unity.Entities.World.Active.GetOrCreateManager<EntityManager>();
			var spawnPosition = new float3(0, 2, 0);

			var rabbit = manager.CreateEntity(Bootstrap.rabbitArchetype);
			manager.SetComponentData(rabbit, new ActorType { Value = Actor.Type.Rabbit });
			manager.SetComponentData(rabbit, new Position { Value = spawnPosition });
			manager.SetComponentData(rabbit, new RandomMove { State = random.NextUInt() });
		}
	}
}
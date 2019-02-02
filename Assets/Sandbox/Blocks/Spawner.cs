namespace Sandbox {
	using Sandbox.Core;
	using Unity.Entities;
	using Unity.Mathematics;
	using Unity.Transforms;

	public class Spawner : Block {
		public Spawner() : base("spawner") { }

		public override Entity CreateEntity(Volume volume, int3 pos) {
			var manager = World.Active.GetOrCreateManager<EntityManager>();
			var entity = manager.CreateEntity(typeof(Position), typeof(RabbitSpawner));
			manager.SetComponentData(entity, new Position { Value = pos + volume.position });
			return entity;
		}
	}
}
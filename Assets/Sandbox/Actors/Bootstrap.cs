namespace Sandbox {
	using Unity.Entities;
	using Unity.Rendering;
	using Unity.Transforms;
	using UnityEngine;
	using UnityEngine.Rendering;

	public class Bootstrap {
		public static EntityArchetype rabbitArchetype;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void PreInitialize() {
			var manager = Unity.Entities.World.Active.GetOrCreateManager<EntityManager>();

			rabbitArchetype = manager.CreateArchetype(
				typeof(ActorType),
				typeof(Collider),
				typeof(FallDown),
				typeof(JumpForward),
				typeof(Position),
				typeof(RandomMove),
				typeof(ServerSide),
				typeof(Velocity)
			);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void Initialize() {
			var manager = Unity.Entities.World.Active.GetOrCreateManager<EntityManager>();
		}
	}
}
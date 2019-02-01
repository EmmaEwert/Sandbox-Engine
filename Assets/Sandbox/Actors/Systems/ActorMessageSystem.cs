namespace Sandbox {
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Transforms;

	public class ActorMessageSystem : JobComponentSystem {
		struct SendJob : IJobProcessComponentDataWithEntity<ActorType, Position, ServerSide> {
			public void Execute(Entity entity, int index, ref ActorType type, ref Position pos, ref ServerSide _) {
				new ActorTransformMessage(entity.Index, (int)type.Value, pos.Value).Broadcast();
			}
		}

		protected override JobHandle OnUpdate(JobHandle dependencies) {
			return new SendJob { }.Schedule(this, dependencies);
		}
	}
}
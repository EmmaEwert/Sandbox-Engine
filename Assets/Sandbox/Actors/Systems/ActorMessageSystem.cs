namespace Sandbox {
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Transforms;

	public class ActorMessageSystem : JobComponentSystem {
		struct SendJob : IJobProcessComponentDataWithEntity<ActorType, Position> {
			public void Execute([ReadOnly] Entity entity, [ReadOnly] int index, [ReadOnly] ref ActorType type, [ReadOnly] ref Position pos) {
				new ActorTransformMessage(index, (int)type.Value, pos.Value).Broadcast();
			}
		}

		protected override JobHandle OnUpdate(JobHandle dependencies) {
			return new SendJob().Schedule(this, dependencies);
		}
	}
}
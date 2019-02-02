namespace Sandbox {
	using Sandbox.Core;
	using Unity.Mathematics;

	public class Lever : Block {
		public override bool opaqueCube => false;

		public Lever() : base("lever", "face=floor,facing=north,powered=false") { }

		public override Box Box(Volume volume, int3 pos) {
			return new Box {
				min = pos + new float3(5, 0, 4) / 16f,
				max = pos + new float3(11, 3, 12) / 16f,
			};
		}

		public override void On(Verb verb, Volume volume, int3 pos) {
			if (verb is Interact) {
				var state = BlockState.blockStates[volume[pos]];
				var lever = BlockManager.Load("lever");
				if (state.properties["powered"]) {
					new PlaceBlockMessage(pos, lever["face=floor,facing=north,powered=false"].id).Send();
					new PlaceBlockMessage(pos + new int3(0, -1, 0), BlockManager.Default("cobblestone").id).Send();
				} else {
					new PlaceBlockMessage(pos, lever["face=floor,facing=north,powered=true"].id).Send();
					new PlaceBlockMessage(pos + new int3(0, -1, 0), BlockManager.Default("spawner").id).Send();
				}
			} else {
				base.On(verb, volume, pos);
			}
		}
	}
}
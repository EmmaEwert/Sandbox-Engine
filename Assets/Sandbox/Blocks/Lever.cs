namespace Sandbox {
	using Sandbox.Core;
	using Unity.Mathematics;

	public class Lever : Block {
		public override bool opaqueCube => false;

		public Lever() : base("lever", "face=floor,facing=north,powered=false") { }

		public override void On(Verb verb, Volume volume, int3 pos) {
			if (verb is Interact) {
				var state = BlockState.blockStates[volume[pos]];
				var lever = BlockManager.Load("lever");
				if (state.properties["powered"]) {
					new PlaceBlockMessage(pos, lever["face=floor,facing=north,powered=false"].id).Send();
					new PlaceBlockMessage(pos + new int3(0, -1, 0), BlockManager.Default("cobblestone").id).Send();
				} else {
					new PlaceBlockMessage(pos, lever["face=floor,facing=north,powered=true"].id).Send();
					new PlaceBlockMessage(pos + new int3(0, -1, 0), BlockManager.Default("stone").id).Send();
				}
			} else {
				base.On(verb, volume, pos);
			}
		}
	}
}
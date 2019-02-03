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
				if (volume[pos, "powered"]) {
					volume[pos.down()] = Block.Find<Cobblestone>().defaultState;
				} else {
					volume[pos.down()] = Block.Find<Spawner>().defaultState;
				}
				volume[pos, "powered"] = !volume[pos, "powered"];
			} else {
				base.On(verb, volume, pos);
			}
		}
	}
}
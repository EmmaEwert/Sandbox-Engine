namespace Sandbox {
	using Sandbox.Core;
	using Unity.Mathematics;

	public class Sand : Block {
		public Sand() : base("sand") { }

		public override void OnUpdated(Volume volume, int3 pos) {
			if (volume[pos + new int3(0, -1, 0)] == 0) {
				volume[pos + new int3(0, -1, 0)] = volume[pos];
				volume[pos] = 0;
			} else if (volume[pos + new int3(0, 0, -1)] == 0 && volume[pos + new int3(0, -1, -1)] == 0) {
				volume[pos + new int3(0, 0, -1)] = volume[pos];
				volume[pos] = 0;
			} else if (volume[pos + new int3(0, 0, 1)] == 0 && volume[pos + new int3(0, -1, 1)] == 0) {
				volume[pos + new int3(0, 0, 1)] = volume[pos];
				volume[pos] = 0;
			} else if (volume[pos + new int3(-1, 0, 0)] == 0 && volume[pos + new int3(-1, -1, 0)] == 0) {
				volume[pos + new int3(-1, 0, 0)] = volume[pos];
				volume[pos] = 0;
			} else if (volume[pos + new int3(1, 0, 0)] == 0 && volume[pos + new int3(1, -1, 0)] == 0) {
				volume[pos + new int3(1, 0, 0)] = volume[pos];
				volume[pos] = 0;
			}
		}
	}
}

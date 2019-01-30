namespace Sandbox {
	using Unity.Mathematics;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class Sand : Block {
		public Sand() : base("sand") { }

		public override void OnPlaced(Volume volume, int3 pos) {
			if (volume[pos + int3(0, -1, 0)] == 0) {
				volume[pos + int3(0, -1, 0)] = volume[pos];
				volume[pos] = 0;
			} else if (volume[pos + int3(0, -1, -1)] == 0) {
				volume[pos + int3(0, -1, -1)] = volume[pos];
				volume[pos] = 0;
			} else if (volume[pos + int3(0, -1, 1)] == 0) {
				volume[pos + int3(0, -1, 1)] = volume[pos];
				volume[pos] = 0;
			} else if (volume[pos + int3(-1, -1, 0)] == 0) {
				volume[pos + int3(-1, -1, 0)] = volume[pos];
				volume[pos] = 0;
			} else if (volume[pos + int3(1, -1, 0)] == 0) {
				volume[pos + int3(1, -1, 0)] = volume[pos];
				volume[pos] = 0;
			}
		}
	}
}

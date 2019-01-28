namespace Sandbox {
	using Unity.Mathematics;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class Chunk {
		public const int Size = 16;
		static int3 PosToBlockIndex = int3(1, Size, Size * Size);

		public ushort[] blocks = new ushort[Size * Size * Size];
		public int3 pos;
		public GameObject gameObject;

		public Chunk(int3 pos) {
			this.pos = pos;
		}

		public ushort this[int3 pos] {
			get => blocks[dot(pos, PosToBlockIndex)];
			set => blocks[dot(pos, PosToBlockIndex)] = value;
		}
	}
}
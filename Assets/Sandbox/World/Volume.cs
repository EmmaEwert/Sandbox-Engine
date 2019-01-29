namespace Sandbox {
	using Unity.Mathematics;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class Volume {
		public const int ChunkDistance = 3;
		public const int MaxSize = (int.MaxValue / (Chunk.Size * Chunk.Size * Chunk.Size * 2)) * Chunk.Size * Chunk.Size * Chunk.Size;

		public Chunk[,,] chunks = new Chunk[ChunkDistance, ChunkDistance, ChunkDistance];
		public GameObject gameObject;

		public Volume() {
			for (var z = 0; z < ChunkDistance; ++z)
			for (var y = 0; y < ChunkDistance; ++y)
			for (var x = 0; x < ChunkDistance; ++x) {
				chunks[x, y, z] = new Chunk(int3(x, y, z) * Chunk.Size);
			}
		}

		public ushort this[int3 pos] {
			get => ChunkAt(pos)?[(pos + MaxSize) % Chunk.Size] ?? 0;
			set {
				if (this[pos] == value) { return; }
				var chunk = ChunkAt(pos);
				chunk[(pos + MaxSize) % Chunk.Size] = value;
			}
		}

		public Chunk ChunkAt(int3 pos) {
			pos = ((pos + MaxSize) / Chunk.Size) % (ChunkDistance * 1);
			return chunks[pos.x, pos.y, pos.z];
		}

		public void ChunkAt(int3 pos, Chunk chunk) {
			pos = ((pos + MaxSize) / Chunk.Size) % (ChunkDistance * 1);
			chunks[pos.x, pos.y, pos.z] = chunk;
		}
	}
}
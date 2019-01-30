namespace Sandbox {
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class Volume {
		public const int ChunkDistance = 8;
		public const int SimDistance = ChunkDistance * Chunk.Size;
		public const int MaxSize = (int.MaxValue / (Chunk.Size * Chunk.Size * Chunk.Size * 2)) * Chunk.Size * Chunk.Size * Chunk.Size;

		public ushort id;
		public Chunk[,,] chunks = new Chunk[ChunkDistance, ChunkDistance, ChunkDistance];
		public GameObject gameObject;

		public Volume() {
			for (var z = 0; z < ChunkDistance; ++z)
			for (var y = 0; y < ChunkDistance; ++y)
			for (var x = 0; x < ChunkDistance; ++x) {
				chunks[x, y, z] = new Chunk(int3(x, y, z) * Chunk.Size, this);
			}
		}

		public ushort this[int3 pos] {
			get => ChunkAt(pos)?[(pos + MaxSize) % Chunk.Size] ?? 0;
			set {
				//if (this[pos] == value) { return; }
				var chunk = ChunkAt(pos);
				chunk[(pos + MaxSize) % Chunk.Size] = value;
			}
		}

		public void Update() {
			for (var z = 0; z < ChunkDistance; ++z)
			for (var y = 0; y < ChunkDistance; ++y)
			for (var x = 0; x < ChunkDistance; ++x) {
				if (chunks[x, y, z].dirty) {
					chunks[x, y, z].PreUpdate(this);
				}
			}
			for (var z = 0; z < ChunkDistance; ++z)
			for (var y = 0; y < ChunkDistance; ++y)
			for (var x = 0; x < ChunkDistance; ++x) {
				if (chunks[x, y, z].dirty) {
					chunks[x, y, z].Update(this);
					new ChunkMessage(id, chunks[x, y, z]).Broadcast();
				}
			}
		}

		public void Set(int3 pos, Chunk.Flag flag) {
			ChunkAt(pos).Set((pos + MaxSize) % Chunk.Size, flag);
		}

		public void Generate() {
			var handles = new JobHandle[ChunkDistance * ChunkDistance * ChunkDistance];
			for (var z = 0; z < ChunkDistance; ++z)
			for (var y = 0; y < ChunkDistance; ++y)
			for (var x = 0; x < ChunkDistance; ++x) {
				handles[x + y * ChunkDistance + z * ChunkDistance * ChunkDistance] =
					chunks[x, y, z].Generate();
			}
			for (var i = 0; i < handles.Length; ++i) {
				handles[i].Complete();
				chunks[i % ChunkDistance, i / ChunkDistance % ChunkDistance, i / ChunkDistance / ChunkDistance].AssignGeneratedIDs();
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
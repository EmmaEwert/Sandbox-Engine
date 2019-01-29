namespace Sandbox {
	using System.Collections.Generic;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class World {
		public Dictionary<ushort, Volume> volumes = new Dictionary<ushort, Volume>();
		static Queue<Chunk> dirtyChunks = new Queue<Chunk>();
		
		public static void OnReceive(VolumeMessage message) {
			var volume = GameClient.world.volumes[message.id] = new Volume();
			volume.gameObject = new GameObject("Volume");
			volume.gameObject.transform.position = Vector3.one * -Volume.ChunkDistance * Chunk.Size / 2;
			foreach (var chunk in volume.chunks) {
				var gameObject = chunk.gameObject = new GameObject($"Chunk {chunk.pos}", typeof(MeshFilter), typeof(MeshRenderer));
				gameObject.transform.parent = volume.gameObject.transform;
				gameObject.transform.localPosition = float3(chunk.pos);
				gameObject.GetComponent<MeshRenderer>().sharedMaterial = WorldManager.BlockMaterial;
			}
		}

		public static void OnReceive(ChunkMessage message) {
			Benchmark.Benchmark.StartWatch("Update geometry");
			var volume = GameClient.world.volumes[message.volumeID];
			var pos = message.pos;
			var chunk = volume.ChunkAt(pos);
			chunk.ids = message.blocks;
			chunk.UpdateGeometry(volume);
			Benchmark.Benchmark.StopWatch("Update geometry");
			var neighbors = new [] {
				volume.ChunkAt(pos + int3(0, 0, -Chunk.Size)),
				volume.ChunkAt(pos + int3(0, 0,  Chunk.Size)),
				volume.ChunkAt(pos + int3(0, -Chunk.Size, 0)),
				volume.ChunkAt(pos + int3(0,  Chunk.Size, 0)),
				volume.ChunkAt(pos + int3(-Chunk.Size, 0, 0)),
				volume.ChunkAt(pos + int3( Chunk.Size, 0, 0))
			};
			foreach (var neighbor in neighbors) {
				if (!dirtyChunks.Contains(neighbor)) {
					dirtyChunks.Enqueue(neighbor);
				}
			}
			Benchmark.Benchmark.StartWatch("Update colliders");
			foreach (Transform child in chunk.gameObject.transform) {
				GameObject.Destroy(child.gameObject);
			}
			for (var z = pos.z; z < pos.z + Chunk.Size; ++z)
			for (var y = pos.y; y < pos.y + Chunk.Size; ++y)
			for (var x = pos.x; x < pos.x + Chunk.Size; ++x) {
				if (volume[int3(x, y, z)] == 0) { continue; }
				if (x > pos.x && volume[int3(x - 1, y, z)] != 0)
					if (y > pos.y && volume[int3(x, y - 1, z)] != 0)
						if (z > pos.z && volume[int3(x, y, z - 1)] != 0)
							if (x < pos.x + Chunk.Size - 1 && volume[int3(x + 1, y, z)] != 0)
								if (y < pos.y + Chunk.Size - 1 && volume[int3(x, y + 1, z)] != 0)
									if (z < pos.z + Chunk.Size - 1 && volume[int3(x, y, z + 1)] != 0) {
										continue;
									}
				var block = GameObject.Instantiate(WorldManager.BlockPrefab);
				block.transform.parent = chunk.gameObject.transform;
				block.transform.localPosition = float3(
					(x + Volume.MaxSize) % Chunk.Size,
					(y + Volume.MaxSize) % Chunk.Size,
					(z + Volume.MaxSize) % Chunk.Size
				);
			}
			Benchmark.Benchmark.StopWatches("ChunkMessage");
		}

		public void Update() {
			if (dirtyChunks.Count > 0) {
				dirtyChunks.Dequeue().UpdateGeometry(volumes[0]);
			}
		}

		internal void Generate() {
			volumes[0] = new Volume();
			for (var z = 0; z < Volume.ChunkDistance * Chunk.Size; ++z)
			for (var y = 0; y < Volume.ChunkDistance * Chunk.Size; ++y)
			for (var x = 0; x < Volume.ChunkDistance * Chunk.Size; ++x) {
				if (y < Chunk.Size * Volume.ChunkDistance / 2) {
					volumes[0][int3(x, y, z)] = BlockManager.Default("sand").id;
				}
			}
		}
	}
}
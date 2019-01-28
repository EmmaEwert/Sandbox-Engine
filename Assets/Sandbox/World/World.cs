namespace Sandbox {
	using System.Collections.Generic;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class World {
		public Dictionary<ushort, Volume> volumes = new Dictionary<ushort, Volume>();
		
		internal void Generate() {
			volumes[0] = new Volume();
			for (var z = 0; z < Volume.ChunkDistance * Chunk.Size; ++z)
			for (var y = 0; y < Volume.ChunkDistance * Chunk.Size; ++y)
			for (var x = 0; x < Volume.ChunkDistance * Chunk.Size; ++x) {
				if (y < Chunk.Size * Volume.ChunkDistance / 2) {
					volumes[0][int3(x, y, z)] = 1;
				}
			}
		}

		public static void OnReceive(VolumeMessage message) {
			var volume = GameClient.world.volumes[message.id] = new Volume();
			volume.gameObject = new GameObject("Volume");
			volume.gameObject.transform.position = Vector3.one * -Volume.ChunkDistance * Chunk.Size / 2;
			foreach (var chunk in volume.chunks) {
				chunk.gameObject = new GameObject($"Chunk {chunk.pos}");
				chunk.gameObject.transform.parent = volume.gameObject.transform;
				chunk.gameObject.transform.localPosition = float3(chunk.pos);
			}
		}

		public static void OnReceive(ChunkMessage message) {
			var volume = GameClient.world.volumes[message.volumeID];
			var pos = message.pos;
			var chunk = volume.ChunkAt(pos);
			foreach (Transform child in chunk.gameObject.transform) {
				GameObject.Destroy(child.gameObject);
			}
			chunk.blocks = message.blocks;
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
		}
	}
}
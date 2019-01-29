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
		}

		public void Update() {
			//if (dirtyChunks.Count > 0) {
				//dirtyChunks.Dequeue().UpdateGeometry(volumes[0]);
			//}
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
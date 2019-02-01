namespace Sandbox.Core {
	using System.Collections.Generic;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class Universe {
		public Dictionary<ushort, Volume> volumes = new Dictionary<ushort, Volume>();
		static Queue<Chunk> dirtyChunks = new Queue<Chunk>();
		
		public static void OnReceive(VolumeMessage message) {
			var volume = GameClient.universe.volumes[message.id] = new Volume();
			volume.id = message.id;
			volume.gameObject = new GameObject("Volume");
			volume.gameObject.transform.position = Vector3.one * -Volume.ChunkDistance * Chunk.Size / 2;
			foreach (var chunk in volume.chunks) {
				var gameObject = chunk.gameObject = new GameObject($"Chunk {chunk.pos}", typeof(MeshFilter), typeof(MeshRenderer));
				gameObject.transform.parent = volume.gameObject.transform;
				gameObject.transform.localPosition = float3(chunk.pos);
				gameObject.GetComponent<MeshRenderer>().sharedMaterial = UniverseManager.BlockMaterial;
			}
		}

		public static void OnReceive(ChunkMessage message) {
			//Benchmark.Benchmark.StartWatch("Update geometry");
			var volume = GameClient.universe.volumes[message.volumeID];
			var pos = message.pos;
			var chunk = volume.ChunkAt(pos);
			chunk.IDs(message.ids);
			chunk.UpdateGeometry(volume);
			//Benchmark.Benchmark.StopWatches("ChunkMessage");
		}

		public void Update() {
			foreach (var volume in volumes.Values) {
				volume.Update();
			}
		}

		internal void Generate() {
			volumes[0] = new Volume();
			volumes[0].Generate();
			volumes[0].id = 0;
			return;
		}


	}
}
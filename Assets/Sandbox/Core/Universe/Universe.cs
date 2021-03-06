namespace Sandbox.Core {
	using System.Collections.Generic;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class Universe {
		public Dictionary<ushort, Volume> volumes = new Dictionary<ushort, Volume>();
		static Queue<Chunk> dirtyChunks = new Queue<Chunk>();
		
		public void Add(VolumeMessage message) {
			var volume = volumes[message.id] = new Volume(server: false);
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

		public void Update(ChunkMessage message) {
			var volume = volumes[message.volumeID];
			var pos = message.pos;
			var chunk = volume.ChunkAt(pos);
			chunk.IDs(message.ids);
		}

		public void Update() {
			foreach (var volume in volumes.Values) {
				if (volume.server) {
					volume.ServerUpdate();
				} else {
					volume.ClientUpdate();
				}
			}
		}

		internal void Generate() {
			volumes[0] = new Volume(server: true);
			volumes[0].Generate();
			volumes[0].id = 0;
			return;
		}


	}
}
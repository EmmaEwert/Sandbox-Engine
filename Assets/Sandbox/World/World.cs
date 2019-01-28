namespace Sandbox {
	using Sandbox.Net;
	using System.Collections.Generic;
	using UnityEngine;

	public class World {
		internal const ushort Size = 16;
		internal ushort[,,] blocks = new ushort[Size, Size, Size];
		public GameObject gameObject;

		Dictionary<ushort, Volume> volumes = new Dictionary<ushort, Volume>();
		
		internal void Generate() {
			for (var z = 0; z < Size; ++z)
			for (var y = 0; y < Size; ++y)
			for (var x = 0; x < Size; ++x) {
				if (y < Size / 2) {
					blocks[x, y, z] = 1;
				}
			}
		}

		public static void OnReceive(ChunkMessage message) {

		}

		internal static void OnReceive(WorldPartMessage message) {
			if (GameClient.world == null) {
				GameClient.world = new World();
				GameClient.world.gameObject = new GameObject("World");
				GameClient.world.gameObject.transform.position = Vector3.one * -8f;
			} else {
				foreach (Transform child in GameClient.world.gameObject.transform) {
					GameObject.Destroy(child.gameObject);
				}
			}
			for (var z = 0; z < 16; ++z)
			for (var y = 0; y < 16; ++y)
			for (var x = 0; x < 16; ++x) {
				GameClient.world.blocks[x, y, z] = message.blocks[x, y, z];
			}
			for (var z = 0; z < Size; ++z)
			for (var y = 0; y < Size; ++y)
			for (var x = 0; x < Size; ++x) {
				if (GameClient.world.blocks[x, y, z] == 0) { continue; }
				if (x > 0 && GameClient.world.blocks[x - 1, y, z] != 0)
					if (y > 0 && GameClient.world.blocks[x, y - 1, z] != 0)
						if (z > 0 && GameClient.world.blocks[x, y, z - 1] != 0)
							if (x < Size - 1 && GameClient.world.blocks[x + 1, y, z] != 0)
								if (y < Size - 1 && GameClient.world.blocks[x, y + 1, z] != 0)
									if (z < Size - 1 && GameClient.world.blocks[x, y, z + 1] != 0) {
										continue;
									}
				var block = GameObject.Instantiate(WorldManager.BlockPrefab);
				block.transform.parent = GameClient.world.gameObject.transform;
				block.transform.localPosition = new Vector3(x, y, z);
			}
		}
	}
}
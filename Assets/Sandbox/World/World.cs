namespace Sandbox {
	using Sandbox.Net;
	using UnityEngine;

	public class World {
		internal const ushort Size = 16;
		internal ushort[,,] blocks = new ushort[Size, Size, Size];
		public GameObject gameObject;
		
		internal void Generate() {
			for (var z = 0; z < Size; ++z)
			for (var y = 0; y < Size; ++y)
			for (var x = 0; x < Size; ++x) {
				if (y < Size / 2) {
					blocks[x, y, z] = 1;
				}
			}
		}

		internal static void OnReceive(WorldPartMessage message) {
			if (Client.world == null) {
				Client.world = new World();
				Client.world.gameObject = new GameObject("World");
				Client.world.gameObject.transform.position = Vector3.one * -8f;
			} else {
				foreach (Transform child in Client.world.gameObject.transform) {
					GameObject.Destroy(child.gameObject);
				}
			}
			for (var z = 0; z < 16; ++z)
			for (var y = 0; y < 16; ++y)
			for (var x = 0; x < 16; ++x) {
				Client.world.blocks[x, y, z] = message.blocks[x, y, z];
			}
			for (var z = 0; z < Size; ++z)
			for (var y = 0; y < Size; ++y)
			for (var x = 0; x < Size; ++x) {
				if (Client.world.blocks[x, y, z] == 0) { continue; }
				if (x > 0 && Client.world.blocks[x - 1, y, z] != 0)
					if (y > 0 && Client.world.blocks[x, y - 1, z] != 0)
						if (z > 0 && Client.world.blocks[x, y, z - 1] != 0)
							if (x < Size - 1 && Client.world.blocks[x + 1, y, z] != 0)
								if (y < Size - 1 && Client.world.blocks[x, y + 1, z] != 0)
									if (z < Size - 1 && Client.world.blocks[x, y, z + 1] != 0) {
										continue;
									}
				var block = GameObject.Instantiate(WorldManager.BlockPrefab);
				block.transform.parent = Client.world.gameObject.transform;
				block.transform.localPosition = new Vector3(x, y, z);
			}
		}
	}
}
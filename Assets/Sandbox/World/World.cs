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
			var offsetX = message.offsetX;
			var offsetY = message.offsetY;
			var offsetZ = message.offsetZ;
			for (var z = offsetZ; z < offsetZ + 8; ++z)
			for (var y = offsetY; y < offsetY + 8; ++y)
			for (var x = offsetX; x < offsetX + 8; ++x) {
				Client.world.blocks[x, y, z] = message.blocks[x % 8, y % 8, z % 8];
			}
			for (var z = 0; z < Size; ++z)
			for (var y = 0; y < Size; ++y)
			for (var x = 0; x < Size; ++x) {
				if (Client.world.blocks[x, y, z] == 0) { continue; }
				var block = GameObject.Instantiate(WorldManager.BlockPrefab);
				//var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
				block.transform.parent = Client.world.gameObject.transform;
				block.transform.localPosition = new Vector3(x, y, z);
			}
		}
	}
}
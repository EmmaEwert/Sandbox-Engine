namespace Sandbox {
	using Sandbox.Core;
	using Sandbox.Net;
	using Unity.Mathematics;
	using UnityEngine;

	public class Game : MonoBehaviour {
		public string playerName {
			get => PlayerPrefs.GetString("name", "Emma");
			set => PlayerPrefs.SetString("name", value);
		}
		public string ip { get; set; } = "82.180.25.150";
		public bool server { get; set; } = false;

		void ReplaceBlock(PlaceBlockMessage message) {
			if (math.any(message.blockPosition < 0) || math.any(message.blockPosition >= Volume.ChunkDistance * Chunk.Size)) { return; }
			var volume = Core.Server.universe.volumes[0];
			volume[message.blockPosition] = message.state;
		}

		void Start() {
			Core.Common.Start();
			if (server) {
				Net.Server.Listen<PlaceBlockMessage>(ReplaceBlock);
				Core.Server.universe.Generate();
				Core.Server.Start(playerName);
			} else {
				Core.Client.Start(ip, playerName);
			}
		}

		void Update() {
			Core.Common.Update();
			if (server) {
				Core.Server.Update();
			} else {
				Core.Client.Update();
			}
		}

		void OnDestroy() {
			if (server) {
				Core.Server.Stop();
			} else {
				Core.Client.Stop();
			}
		}
	}
}
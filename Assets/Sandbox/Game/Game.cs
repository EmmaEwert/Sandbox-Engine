namespace Sandbox {
	using Sandbox.Net;
	using UnityEngine;

	public class Game : MonoBehaviour {
		public string playerName {
			get => PlayerPrefs.GetString("name", "Emma");
			set => PlayerPrefs.SetString("name", value);
		}
		public string ip { get; set; } = "82.180.25.150";
		public bool server { get; set; } = false;

		void Start() {
			BlockManager.Start();
			ReliableMessage.Start();
			if (server) {
				GameServer.Start(playerName);
			} else {
				GameClient.Start(ip, playerName);
			}
		}

		void Update() {
			Message.Update();
			ReliableMessage.Update();
			if (server) {
				GameServer.Update();
			} else {
				GameClient.Update();
			}
		}

		void OnDestroy() {
			if (server) {
				GameServer.Stop();
			} else {
				GameClient.Stop();
			}
		}
	}
}
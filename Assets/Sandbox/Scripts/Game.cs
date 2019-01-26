namespace Sandbox {
	using Sandbox.Net;
	using UnityEngine;

	public class Game : MonoBehaviour {
		public string playerName { get; set; } = "Emma";
		public string ip { get; set; } = "82.180.25.150";
		public bool server { get; set; } = false;

		void Start() {
			if (server) {
				Server.world = new World();
				Server.world.Generate();
				Server.Start(playerName);
			} else {
				Client.Start(ip, playerName);
			}
		}

		void Update() {
			if (server) {
				Server.Update();
			} else {
				Client.Update();
			}
		}

		void OnDestroy() {
			if (server) {
				Server.Stop();
			} else {
				Client.Stop();
			}
		}
	}
}
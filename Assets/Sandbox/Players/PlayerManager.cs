namespace Sandbox {
	using Sandbox.Net;
	using System.Collections.Generic;
	using UnityEngine;

	public class PlayerManager : MonoBehaviour {
		Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();
		GameObject localPlayer;

		public GameObject localPrefab;
		public GameObject remotePrefab;

		void OnReceive(ConnectMessage message) {
			if (message.local) {
				localPlayer = Instantiate(localPrefab);
			} else {
				players[message.id] = Instantiate(remotePrefab);
			}
		}

		void OnServerReceive(TransformMessage message) {
			message.Broadcast();
		}
		
		void OnClientReceive(TransformMessage message) {
			if (!message.local) {
				var player = players[message.id].transform;
				player.position = message.position;
				player.rotation = message.rotation;
			}
		}

		void Awake() {
			Message.RegisterClientHandler<ConnectMessage>(OnReceive);
			Message.RegisterClientHandler<TransformMessage>(OnClientReceive);
			Message.RegisterServerHandler<TransformMessage>(OnServerReceive);
		}
	}
}
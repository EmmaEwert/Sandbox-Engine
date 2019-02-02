namespace Sandbox {
	using Sandbox.Net;
	using System.Collections.Generic;
	using UnityEngine;

	public class PlayerManager : MonoBehaviour {
		Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();
		GameObject localPlayer;

		public GameObject localPrefab;
		public GameObject remotePrefab;

		void InstantiatePlayer(JoinMessage message) {
			if (message.local) {
				localPlayer = Instantiate(localPrefab);
			} else {
				players[message.id] = Instantiate(remotePrefab);
			}
		}

		void SyncTransform(TransformMessage message) {
			if (!message.local) {
				var player = players[message.id].transform;
				player.position = message.position;
				player.rotation = message.rotation;
			}
		}

		void Awake() {
			Client.Listen<JoinMessage>(InstantiatePlayer);
			Client.Listen<TransformMessage>(SyncTransform);
			Server.Listen<TransformMessage>(message => message.Broadcast());
		}
	}
}
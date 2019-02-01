namespace Sandbox {
	using System.Collections.Generic;
	using Sandbox.Net;
	using UnityEngine;

	public class ActorManager : MonoBehaviour {
		public Transform rabbitPrefab;
		Dictionary<int, Transform> actors = new Dictionary<int, Transform>();

		void OnReceive(ActorTransformMessage message) {
			if (!actors.TryGetValue(message.id, out var actor)) {
				switch ((Actor.Type)message.type) {
					case Actor.Type.Rabbit: actors[message.id] = actor = Instantiate(rabbitPrefab); break;
					default: return;
				}
			}
			actor.position = message.pos;

		}

		void Awake() {
			Message.RegisterClientHandler<ActorTransformMessage>(OnReceive);
		}
	}
}
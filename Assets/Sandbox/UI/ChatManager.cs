namespace Sandbox {
	using System.Collections.Generic;
	using System.Linq;
	using Sandbox.Net;
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Text))]
	public class ChatManager : MonoBehaviour {
		static List<string> messages = new List<string>();
		static Text chat;

		public static void Add(string text) {
			messages.Add(text);
		}

		public void Send(string text) {
			if (string.IsNullOrEmpty(text)) { return; }
			new ChatMessage(text).Send();
		}

		void OnServerReceive(ChatMessage message) {
			message.Broadcast();
		}

		void OnClientReceive(ChatMessage message) {
			Add($"{message.name}: {message.text}");
		}

		void OnReceive(JoinMessage message) {
			Add($"{message.name} connected with ID {message.id}");
		}

		void Start() {
			Message.RegisterServerHandler<ChatMessage>(OnServerReceive);
			Message.RegisterClientHandler<ChatMessage>(OnClientReceive);
			Message.RegisterClientHandler<JoinMessage>(OnReceive);
			chat = GetComponent<Text>();
			Add("Hi~");
		}

		void Update() {
			chat.text = messages
				.Skip(messages.Count - 10)
				.Take(10)
				.Where(m => m != null)
				.Aggregate("", (a, b) => $"{a}\n{b}");
		}
	}
}
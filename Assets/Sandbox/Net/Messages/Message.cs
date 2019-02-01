namespace Sandbox.Net {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Sandbox.Core;
	using UnityEngine;

	public abstract class Message {
		protected static List<Message> clientReceivedMessages = new List<Message>();
		protected static List<Message> serverReceivedMessages = new List<Message>();
		static List<System.Type> types;

		internal static List<System.Type> Types =>
			types = types ?? Reflector.ImplementationsOf<Message>();
		protected static int StringSize(string text) =>
			sizeof(int) + Encoding.UTF8.GetByteCount(text);
		static Dictionary<System.Type, Action<Message>> onServerReceive = new Dictionary<Type, Action<Message>>();
		static Dictionary<System.Type, Action<Message>> onClientReceive = new Dictionary<Type, Action<Message>>();

		internal int connection;

		protected abstract int length { get; }

		public static void Update() {
			var serverMessageCount = serverReceivedMessages.Count;
			for (var i = 0; i < serverMessageCount; ++i) {
				serverReceivedMessages[i].OnReceive(server: true);
			}
			serverReceivedMessages.RemoveRange(0, serverMessageCount);

			var clientMessageCount = clientReceivedMessages.Count;
			for (var i = 0; i < clientMessageCount; ++i) {
				clientReceivedMessages[i]?.OnReceive(server: false);
			}
			clientReceivedMessages.RemoveRange(0, clientMessageCount);
		}

		public static void RegisterServerHandler<T>(Action<T> onReceive) where T : Message {
			if (onServerReceive.TryGetValue(typeof(T), out var handler)) {
				onServerReceive[typeof(T)] = handler + new Action<Message>(o => onReceive((T)o));
			} else {
				onServerReceive[typeof(T)] = new Action<Message>(o => onReceive((T)o));
			}
		}

		public static void RegisterClientHandler<T>(Action<T> onReceive) where T : Message {
			if (onClientReceive.TryGetValue(typeof(T), out var handler)) {
				onClientReceive[typeof(T)] = handler + new Action<Message>(o => onReceive((T)o));
			} else {
				onClientReceive[typeof(T)] = new Action<Message>(o => onReceive((T)o));
			}
		}

		///<summary>Send from client to server.</summary>
		public virtual void Send() {
			if (this is IClientMessage message) {
				using (var writer = new Writer()) {
					writer.Write((ushort)Types.IndexOf(this.GetType()));
					message.Write(writer);
					var bytes = writer.ToArray();
					Client.Send(bytes);
				}
			}
		}

		///<summary>Send from server to client.</summary>
		public virtual void Send(int connection) {
			if (this is IServerMessage message) {
				using (var writer = new Writer()) {
					writer.Write((ushort)Types.IndexOf(this.GetType()));
					message.Write(writer);
					var bytes = writer.ToArray();
					Server.Send(bytes, connection);
				}
			}
		}

		///<summary>Broadcast from server to all clients.</summary>
		public virtual void Broadcast() {
			if (this is IServerMessage message) {
				using (var writer = new Writer()) {
					writer.Write((ushort)Types.IndexOf(this.GetType()));
					message.Write(writer);
					var bytes = writer.ToArray();
					Server.Broadcast(bytes);
				}
			}
		}

		///<summary>Receive on server from client.</summary>
		internal virtual void Receive(Reader reader, int connection) {
			this.connection = connection;
			if (this is IClientMessage message) {
				message.Read(reader);
				serverReceivedMessages.Add(this);
			} else {
				Debug.LogWarning($"Server received illegal message {GetType()}, ignoring.");
			}
		}

		///<summary>Receive on client from server.</summary>
		internal virtual void Receive(Reader reader) {
			if (this is IServerMessage message) {
				message.Read(reader);
				clientReceivedMessages.Add(this);
			} else {
				Debug.LogWarning($"Client received illegal message {GetType()}, ignoring.");
			}
		}

		void OnReceive(bool server = false) {
			var onReceive = server ? onServerReceive : onClientReceive;
			if (onReceive.TryGetValue(GetType(), out var handler)) {
				handler(this);
			} else {
				Debug.Log($"No handlers for {GetType()}, server: {server}, ignoring…");
			}
		}
	}
}
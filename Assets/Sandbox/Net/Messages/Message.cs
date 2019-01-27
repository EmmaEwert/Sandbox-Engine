namespace Sandbox.Net {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using UnityEngine;

	public abstract class Message {
		public int connection;

		protected static int StringSize(string text) =>
			sizeof(int) + Encoding.UTF8.GetByteCount(text);

		static List<System.Type> types;
		internal static List<System.Type> Types =>
			types = types ?? Reflector.ImplementationsOf<Message>();
		static Dictionary<System.Type, Action<Message>> onServerReceive = new Dictionary<Type, Action<Message>>();
		static Dictionary<System.Type, Action<Message>> onClientReceive = new Dictionary<Type, Action<Message>>();

		protected static List<Message> clientReceivedMessages = new List<Message>();
		protected static List<Message> serverReceivedMessages = new List<Message>();

		protected abstract int length { get; }

		public static void Update() {
			var serverMessageCount = serverReceivedMessages.Count;
			for (var i = 0; i < serverMessageCount; ++i) {
				serverReceivedMessages[i].OnReceive(server: true);
			}
			serverReceivedMessages.RemoveRange(0, serverMessageCount);

			var clientMessageCount = clientReceivedMessages.Count;
			for (var i = 0; i < clientMessageCount; ++i) {
				clientReceivedMessages[i].OnReceive(server: false);
			}
			clientReceivedMessages.RemoveRange(0, clientMessageCount);
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
		public virtual void Receive(Reader reader, int connection) {
			this.connection = connection;
			if (this is IClientMessage message) {
				message.Read(reader);
				serverReceivedMessages.Add(this);
				//OnReceive(server: true);
			} else {
				Debug.LogWarning($"Server received illegal message {GetType()}, ignoring.");
			}
		}

		///<summary>Receive on client from server.</summary>
		public virtual void Receive(Reader reader) {
			if (this is IServerMessage message) {
				message.Read(reader);
				clientReceivedMessages.Add(this);
				//OnReceive(server: false);
			} else {
				Debug.LogWarning($"Client received illegal message {GetType()}, ignoring.");
			}
		}

		public static void RegisterServerHandler<T>(Action<T> onReceive) where T : Message {
			if (Message.onServerReceive.TryGetValue(typeof(T), out var handler)) {
				onClientReceive[typeof(T)] = handler + new Action<Message>(o => onReceive((T)o));
			} else {
				Message.onServerReceive[typeof(T)] = new Action<Message>(o => onReceive((T)o));
			}
		}

		public static void RegisterClientHandler<T>(Action<T> onReceive) where T : Message {
			if (Message.onClientReceive.TryGetValue(typeof(T), out var handler)) {
				onClientReceive[typeof(T)] = handler + new Action<Message>(o => onReceive((T)o));
			} else {
				Message.onClientReceive[typeof(T)] = new Action<Message>(o => onReceive((T)o));
			}
		}

		public virtual void OnReceive(bool server = false) {
			var onReceive = server ? onServerReceive : onClientReceive;
			if (onReceive.TryGetValue(GetType(), out var handler)) {
				handler(this);
			}
		}
	}
}
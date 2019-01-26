namespace Sandbox.Net {
	using System;
	using System.Collections.Generic;
	using System.Text;

	public abstract class Message {
		public int connection;

		protected static int StringSize(string text) =>
			sizeof(int) + Encoding.UTF8.GetByteCount(text);

		static List<System.Type> types;
		internal static List<System.Type> Types =>
			types = types ?? Reflector.ImplementationsOf<Message>();
		static Dictionary<System.Type, Action<Message>> onServerReceive = new Dictionary<Type, Action<Message>>();
		static Dictionary<System.Type, Action<Message>> onClientReceive = new Dictionary<Type, Action<Message>>();

		protected abstract int length { get; }

		///<summary>Send from client to server</summary>
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

		///<summary>Send from server to client</summary>
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

		///<summary>Broadcast from server to all clients</summary>
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
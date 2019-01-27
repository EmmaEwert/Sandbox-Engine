namespace Sandbox.Net {
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using UnityEngine;

	public abstract class ReliableMessage : Message {
		static int[] Delays = { 1, 1, 2, 3, 5, 8, 13, 21 };

		// Client
		static int incomingSequence = 0;
		static int outgoingSequence = 0;
		static Dictionary<int, ReliableMessage> clientMessages = new Dictionary<int, ReliableMessage>();
		static Dictionary<(int connection, int sequence), ReliableMessage> serverMessages = new Dictionary<(int, int), ReliableMessage>();

		// Server
		static Dictionary<int, int> incomingSequences = new Dictionary<int, int>();
		static Dictionary<int, int> outgoingSequences = new Dictionary<int, int>();
		static Dictionary<int, ReliableMessage> waitingOnClient = new Dictionary<int, ReliableMessage>();
		static Dictionary<(int connection, int sequence), ReliableMessage> waitingOnServer = new Dictionary<(int connection, int sequence), ReliableMessage>();

		static float time;
		int sequence;
		float timestamp;
		int resends = 0;

		public static void Start() {
			time = Time.realtimeSinceStartup;
			Message.RegisterClientHandler<ActualAckMessage>(OnClientReceive);
			Message.RegisterServerHandler<ActualAckMessage>(OnServerReceive);
		}

		///<summary>Resend stale unacknowledged messages.</summary>
		public static new void Update() {
			time = Time.realtimeSinceStartup;

			var clientSequences = clientMessages.Keys.ToList();
			clientSequences.Sort();
			foreach (var sequence in clientSequences) {
				var message = clientMessages[sequence];
				if (message.resends == 8) {
					Client.Stop();
					return;
				}
				if (time - message.timestamp > Delays[message.resends]) {
					Debug.Log($"No ACK from server, SEQ {sequence}, resending…");
					message.Resend();
				}
			}

			var serverSequences = serverMessages.Keys.ToList();
			serverSequences.Sort();
			foreach (var client in serverSequences) {
				var message = serverMessages[client];
				if (message.resends == 8) {
					// TODO: disconnect client
					return;
				}
				if (time - message.timestamp > Delays[message.resends]) {
					Debug.Log($"No ACK from client {client.connection}, SEQ {client.sequence}, resending…");
					message.Resend(client.connection);
				}
			}
		}

		///<summary>Send from client to server.</summary>
		public override void Send() {
			if (this is IClientMessage message) {
				timestamp = time;
				clientMessages[outgoingSequence] = this;
				using (var writer = new Writer()) {
					writer.Write((ushort)Types.IndexOf(this.GetType()));
					writer.Write(outgoingSequence);
					sequence = outgoingSequence;
					++outgoingSequence;
					message.Write(writer);
					var bytes = writer.ToArray();
					Client.Send(bytes);
				}
			}
		}

		///<summary>Resend from client to server.</summary>
		void Resend() {
			if (this is IClientMessage message) {
				timestamp = time;
				++resends;
				using (var writer = new Writer()) {
					writer.Write((ushort)Types.IndexOf(this.GetType()));
					writer.Write(sequence);
					message.Write(writer);
					var bytes = writer.ToArray();
					Client.Send(bytes);
				}
			}
		}

		///<summary>Send from server to client.</summary>
		public override void Send(int connection) {
			if (this is IServerMessage message) {
				timestamp = time;
				if (!outgoingSequences.ContainsKey(connection)) {
					outgoingSequences[connection] = 0;
				}
				serverMessages[(connection, outgoingSequences[connection])] = this;
				using (var writer = new Writer()) {
					writer.Write((ushort)Types.IndexOf(this.GetType()));
					writer.Write(outgoingSequences[connection]);
					sequence = outgoingSequences[connection];
					++outgoingSequences[connection];
					message.Write(writer);
					var bytes = writer.ToArray();
					Server.Send(bytes, connection);
				}
			}
		}

		///<summary>Resend from server to client.</summary>
		void Resend(int connection) {
			if (this is IServerMessage message) {
				timestamp = time;
				++resends;
				using (var writer = new Writer()) {
					writer.Write((ushort)Types.IndexOf(this.GetType()));
					writer.Write(sequence);
					message.Write(writer);
					var bytes = writer.ToArray();
					Server.Send(bytes, connection);
				}
			}
		}

		///<summary>Broadcast from server to all clients.</summary>
		public override void Broadcast() {
			if (this is IServerMessage message) {
				for (var connection = 0; connection < Server.Connections.Length; ++connection) {
					Send(connection);
				}
			}
		}

		///<summary>Receive on server from client.</summary>
		public override void Receive(Reader reader, int connection) {
			if (this is IClientMessage message) {
				reader.Read(out int sequence);
				new ActualAckMessage(sequence).Send(connection);
				if (!incomingSequences.TryGetValue(connection, out var expectedSequence)) {
					incomingSequences[connection] = expectedSequence = 0;
				}
				if (sequence > expectedSequence) {
					Debug.Log($"Out of sequence: was {sequence}, expected {expectedSequence}");
					message.Read(reader);
					waitingOnServer[(connection, sequence)] = this;
					return;
				} else if (sequence < expectedSequence) {
					// Already received and handled this message, disregard.
					return;
				}
				this.connection = connection;
				base.Receive(reader, connection);
				++incomingSequences[connection];
				while (waitingOnServer.TryGetValue((connection, incomingSequences[connection]), out var waitingMessage)) {
					waitingOnServer.Remove((connection, incomingSequences[connection]));
					serverReceivedMessages.Add(waitingMessage);
					++incomingSequences[connection];
					//waitingMessage.Receive(reader, connection);
				}
			}
		}

		///<summary>Receive on client from server.</summary>
		public override void Receive(Reader reader) {
			if (this is IServerMessage message) {
				reader.Read(out int sequence);
				new ActualAckMessage(sequence).Send();
				if (sequence > incomingSequence) {
					Debug.Log($"Out of sequence: was {sequence}, expected {incomingSequence}");
					message.Read(reader);
					waitingOnClient[sequence] = this;
					return;
				} else if (sequence < incomingSequence) {
					// Already received and handled this message, disregard.
					return;
				}
				base.Receive(reader);
				++incomingSequence;
				while (waitingOnClient.TryGetValue(incomingSequence, out var waitingMessage)) {
					waitingOnClient.Remove(incomingSequence);
					clientReceivedMessages.Add(waitingMessage);
					++incomingSequence;
					//message.Receive(reader);
				}
			}
		}

		static void OnClientReceive(ActualAckMessage message) {
			Debug.Log($"ACK from server, SEQ {message.sequence}");
			clientMessages.Remove(message.sequence);
		}

		static void OnServerReceive(ActualAckMessage message) {
			Debug.Log($"ACK from client {message.connection}, SEQ {message.sequence}");
			serverMessages.Remove((message.connection, message.sequence));
		}
	}
}
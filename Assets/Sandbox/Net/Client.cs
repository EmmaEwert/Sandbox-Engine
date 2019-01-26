namespace Sandbox.Net {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Networking.Transport;
	using UnityEngine;

	public static class Client {
		public static World world;
		internal static Dictionary<int, string> players = new Dictionary<int, string>();
		internal static int id;
		static string name;
		static BasicNetworkDriver<IPv4UDPSocket> driver;
		static NativeArray<NetworkConnection> connection;
		static List<NativeArray<byte>> messages = new List<NativeArray<byte>>();
		static JobHandle receiveJobHandle;
		static JobHandle[] sendJobHandles = new JobHandle[0];
		static int incomingSequence;
		static int outgoingSequence;

		public static void Start(string ip, string name) {
			driver = new BasicNetworkDriver<IPv4UDPSocket>(new INetworkParameter[0]);
			connection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);
			Client.name = name;
			var endpoint = new IPEndPoint(IPAddress.Parse(ip), 54889);
			connection[0] = driver.Connect(endpoint);
			ChatManager.Add($"C: Connecting to {endpoint}…");
			Message.RegisterClientHandler<AckMessage>(OnReceive);
			Message.RegisterClientHandler<ConnectMessage>(OnReceive);
			Message.RegisterClientHandler<PingMessage>(OnReceive);
			Message.RegisterClientHandler<WorldPartMessage>(World.OnReceive);
		}

		public static void Stop() {
			receiveJobHandle.Complete();
			for (var i = 0; i < sendJobHandles.Length; ++i) {
				sendJobHandles[i].Complete();
			}
			for (var i = 0; i < messages.Count; ++i) {
				messages[i].Dispose();
			}
			connection.Dispose();
			driver.Dispose();
		}

		public static void Update() {
			// Finish up last frame's jobs
			receiveJobHandle.Complete();
			for (var i = 0; i < sendJobHandles.Length; ++i) {
				sendJobHandles[i].Complete();
				messages[i].Dispose();
			}
			messages.RemoveRange(0, sendJobHandles.Length);

			// Process received messages on the main thread for ease of use.
			for (var i = 0; i < receivedMessages.Count; ++i) {
				receivedMessages[i].OnReceive();
			}
			receivedMessages.Clear();

			// Schedule new network event reception
			var receiveJob = new ReceiveJob {
				driver = driver,
				connection = connection,
			};
			receiveJobHandle = driver.ScheduleUpdate();
			receiveJobHandle = receiveJob.Schedule(receiveJobHandle);

			// Schedule message queue sending
			sendJobHandles = new JobHandle[messages.Count];
			for (var i = 0; i < messages.Count; ++i) {
				sendJobHandles[i] = receiveJobHandle = new SendJob {
					driver = driver,
					connection = connection,
					message = messages[i]
				}.Schedule(receiveJobHandle);
			}
		}

		static void OnReceive(AckMessage message) {
			id = message.id;
		}

		static void OnReceive(ConnectMessage message) {
			players[message.id] = message.name;
		}

		static void OnReceive(PingMessage message) {
			message.Send();
		}

		internal static void Send(byte[] bytes) {
			var data = new NativeArray<byte>(bytes, Allocator.TempJob);
			messages.Add(data);
		}

		static List<Message> receivedMessages = new List<Message>();

		static void Receive(Reader reader) {
			reader.Read(out ushort typeIndex);
			var type = Message.Types[typeIndex];
			var message = (Message)Activator.CreateInstance(type);
			if (message is IServerMessage serverMessage) {
				serverMessage.Read(reader);
				receivedMessages.Add(message);
			} else {
				Debug.LogWarning($"Client received illegal message {message.GetType()}, ignoring.");
			}
		}

		//[BurstCompile]
		struct ReceiveJob : IJob {
			public BasicNetworkDriver<IPv4UDPSocket> driver;
			[ReadOnly] public NativeArray<NetworkConnection> connection;

			public void Execute() {
				if (!connection[0].IsCreated) { return; }

				NetworkEvent.Type command;
				while ((command = connection[0].PopEvent(driver, out var reader)) != NetworkEvent.Type.Empty) {
					switch (command) {
						case NetworkEvent.Type.Connect:
							ChatManager.Add("C: Connected to server");
							new SynMessage(name).Send();
							break;
						case NetworkEvent.Type.Disconnect:
							ChatManager.Add("C: Disconnected from server");
							connection[0] = default(NetworkConnection);
							break;
						case NetworkEvent.Type.Data:
							Receive(new Reader(reader));
							break;
					}
				}
			}
		}

		//[BurstCompile]
		struct SendJob : IJob {
			public BasicNetworkDriver<IPv4UDPSocket> driver;
			[ReadOnly] public NativeArray<NetworkConnection> connection;
			[ReadOnly] public NativeArray<byte> message;

			public void Execute() {
				using (var writer = new DataStreamWriter(message.Length, Allocator.Temp)) {
					writer.Write(message.ToArray());
					driver.Send(connection[0], writer);
				}
			}
		}
	}
}
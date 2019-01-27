namespace Sandbox.Net {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Sockets;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Networking.Transport;
	using UnityEngine;
	using UnityEngine.Assertions;
	using static Unity.Mathematics.math;

	public static class Server {
		const float PacketLoss = 0f;
		public static Dictionary<int, string> players = new Dictionary<int, string>();
		public static World world;
		public static NetworkConnection[] Connections = new NetworkConnection[0];
		static NativeList<NetworkConnection> connections;
		static BasicNetworkDriver<IPv4UDPSocket> driver;
		static List<NativeArray<byte>> broadcasts = new List<NativeArray<byte>>();
		static List<(int connection, NativeArray<byte> data)> messages = new List<(int, NativeArray<byte>)>();
		static JobHandle receiveJobHandle;
		static JobHandle[] sendJobHandles = new JobHandle[0];
		static JobHandle[] broadcastJobHandles = new JobHandle[0];
		static float ping;
		static Unity.Mathematics.Random random = new Unity.Mathematics.Random(1);

		static IPAddress localIP {
			get {
				var host = Dns.GetHostEntry(Dns.GetHostName());
				foreach (var ip in host.AddressList) {
					if (ip.AddressFamily == AddressFamily.InterNetwork) {
						return ip;
					}
				}
				return IPAddress.Loopback;
			}
		}

		///<summary>Start a local server and a client with the given player name.</summary>
		public static void Start(string playerName) {
			var ip = new WebClient().DownloadString("http://bot.whatismyipaddress.com");;
			Debug.Log($"\"{ip}\"");
			driver = new BasicNetworkDriver<IPv4UDPSocket>(new INetworkParameter[0]);
			var endpoint = new IPEndPoint(localIP, 54889);
			if (driver.Bind(endpoint) != 0) {
				ChatManager.Add($"S: Failed to bind {endpoint.Address}:{endpoint.Port}");
			} else {
				driver.Listen();
				ChatManager.Add($"S: Listening on {endpoint.Address}:{endpoint.Port}…");
			}
			connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
			Message.RegisterServerHandler<ButtonMessage>(OnReceive);
			Message.RegisterServerHandler<ConnectClientMessage>(OnReceive);
			Message.RegisterServerHandler<PlaceBlockMessage>(OnReceive);

			Client.Start(localIP.ToString(), playerName);
		}

		///<summary>Clean up server handles.</summary>
		public static void Stop() {
			receiveJobHandle.Complete();
			for (var i = 0; i < sendJobHandles.Length; ++i) {
				sendJobHandles[i].Complete();
				messages[i].data.Dispose();
			}
			messages.RemoveRange(0, sendJobHandles.Length);
			for (var i = 0; i < broadcastJobHandles.Length; ++i) {
				broadcastJobHandles[i].Complete();
				broadcasts[i].Dispose();
			}
			broadcasts.RemoveRange(0, broadcastJobHandles.Length);
			driver.Dispose();
			connections.Dispose();
			Client.Stop();
		}

		///<summary>Update the server state through network IO.</summary>
		public static void Update() {
			// Finish up last frame's jobs
			receiveJobHandle.Complete();
			for (var i = 0; i < sendJobHandles.Length; ++i) {
				sendJobHandles[i].Complete();
				messages[i].data.Dispose();
			}
			messages.RemoveRange(0, sendJobHandles.Length);
			for (var i = 0; i < broadcastJobHandles.Length; ++i) {
				broadcastJobHandles[i].Complete();
				broadcasts[i].Dispose();
			}
			broadcasts.RemoveRange(0, broadcastJobHandles.Length);
			Connections = connections.ToArray();

			// Schedule new network connection and event reception
			var concurrentDriver = driver.ToConcurrent();
			var updateJob = new UpdateJob {
				driver = driver,
				connections = connections
			};
			var receiveJob = new ReceiveJob {
				driver = concurrentDriver,
				connections = connections.AsDeferredJobArray()
			};
			receiveJobHandle = driver.ScheduleUpdate();
			receiveJobHandle = updateJob.Schedule(receiveJobHandle);
			receiveJobHandle = receiveJob.Schedule(connections, 1, receiveJobHandle);

			// Schedule message queue sending
			sendJobHandles = new JobHandle[messages.Count];
			for (var i = 0; i < messages.Count; ++i) {
				receiveJobHandle = new SendJob {
					driver = concurrentDriver,
					connections = connections,
					message = messages[i].data,
					index = messages[i].connection
				}.Schedule(receiveJobHandle);
			}

			// Schedule broadcast queue sending
			broadcastJobHandles = new JobHandle[broadcasts.Count];
			for (var i = 0; i < broadcasts.Count; ++i) {
				receiveJobHandle = broadcastJobHandles[i] = new BroadcastJob {
					driver = concurrentDriver,
					connections = connections.AsDeferredJobArray(),
					message = broadcasts[i]
				}.Schedule(connections, 1, receiveJobHandle);
			}

			// Send pings
			ping += Time.deltaTime;
			if (ping >= 5f) {
				ping = 0f;
				new PingMessage().Broadcast();
			}

			Client.Update();
		}

		internal static void Broadcast(byte[] bytes) {
			var data = new NativeArray<byte>(bytes, Allocator.TempJob);
			broadcasts.Add(data);
		}

		internal static void Send(byte[] bytes, int connection) {
			var data = new NativeArray<byte>(bytes, Allocator.TempJob);
			messages.Add((connection, data));
			//list.Add(data);
		}

		static void OnReceive(ButtonMessage message) {
			if (message.button == 0) {
				world.blocks[message.blockPosition.x, message.blockPosition.y, message.blockPosition.z] = 0;
				new WorldPartMessage(Server.world).Broadcast();
			}
		}

		static void OnReceive(ConnectClientMessage message) {
			players.Add(message.connection, message.name);
			new ConnectServerMessage(message.connection).Send(message.connection);
			new JoinMessage(message.connection, message.name).Broadcast();
			foreach (var player in Server.players) {
				if (player.Key != message.connection) {
					new JoinMessage(player.Key, player.Value).Send(message.connection);
				}
			}
			new WorldPartMessage(Server.world).Send(message.connection);
		}

		static void OnReceive(PlaceBlockMessage message) {
			if (any(message.blockPosition < 0) || any(message.blockPosition >= World.Size)) { return; }
			world.blocks[message.blockPosition.x, message.blockPosition.y, message.blockPosition.z] = 1;
			new WorldPartMessage(world).Broadcast();
		}

		static void Receive(Reader reader, int connection) {
			reader.Read(out ushort typeIndex);
			var type = Message.Types[typeIndex];
			var message = (Message)Activator.CreateInstance(type);
			if (message is ReliableMessage && random.NextFloat() < PacketLoss) { return; }
			message.Receive(reader, connection);
		}

		//[BurstCompile]
		struct UpdateJob : IJob {
			public BasicNetworkDriver<IPv4UDPSocket> driver;
			public NativeList<NetworkConnection> connections;

			public void Execute() {
				// Clean up connections
				for (var i = 0; i < connections.Length; ++i) {
					if (!connections[i].IsCreated) {
						connections.RemoveAtSwapBack(i);
						--i;
					}
				}

				// Accept new connections
				NetworkConnection c;
				while ((c = driver.Accept()) != default(NetworkConnection)) {
					ChatManager.Add($"S: Accepted connection from {c.InternalId}");
					connections.Add(c);
				}
			}
		}

		//[BurstCompile]
		struct ReceiveJob : IJobParallelFor {
			public BasicNetworkDriver<IPv4UDPSocket>.Concurrent driver;
			public NativeArray<NetworkConnection> connections;

			public void Execute(int index) {
				Assert.IsTrue(connections[index].IsCreated);

				NetworkEvent.Type command;
				while ((command = driver.PopEventForConnection(connections[index], out var streamReader)) != NetworkEvent.Type.Empty) {
					switch (command) {
						case NetworkEvent.Type.Disconnect:
							ChatManager.Add($"S: Client disconnected");
							connections[index] = default(NetworkConnection);
							break;
						case NetworkEvent.Type.Data:
							using (var reader = new Reader(streamReader)) {
								Receive(reader, connections[index].InternalId);
							}
							break;
					}
				}
			}
		}

		//[BurstCompile]
		struct SendJob : IJob {
			public BasicNetworkDriver<IPv4UDPSocket>.Concurrent driver;
			[ReadOnly] public NativeList<NetworkConnection> connections;
			[ReadOnly] public NativeArray<byte> message;
			[ReadOnly] public int index;

			public void Execute() {
				using (var writer = new DataStreamWriter(message.Length, Allocator.Temp)) {
					writer.Write(message.ToArray());
					driver.Send(connections[index], writer);
				}
			}
		}

		//[BurstCompile]
		struct BroadcastJob : IJobParallelFor {
			public BasicNetworkDriver<IPv4UDPSocket>.Concurrent driver;
			[ReadOnly] public NativeArray<NetworkConnection> connections;
			[ReadOnly] public NativeArray<byte> message;

			public void Execute(int index) {
				using (var writer = new DataStreamWriter(message.Length, Allocator.Temp)) {
					writer.Write(message.ToArray());
					driver.Send(connections[index], writer);
				}
			}
		}
	}
}
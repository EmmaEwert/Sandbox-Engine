namespace Sandbox {
	using Benchmark;
	using Sandbox.Net;
	using static Unity.Mathematics.math;

	public static class GameServer {
		public static World world = new World();

		public static void Start(string playerName) {
			Message.RegisterServerHandler<ButtonMessage>(OnReceive);
			Message.RegisterServerHandler<ConnectClientMessage>(OnReceive);
			Message.RegisterServerHandler<PlaceBlockMessage>(OnReceive);
			Benchmark.StartWatch("World generation");
			world.Generate();
			Benchmark.StopWatch("World generation");

			Benchmark.StartWatch("Server start");
			Server.Start(playerName);
			Benchmark.StopWatch("Server start");

			Benchmark.StartWatch("Client start");
			GameClient.Start(Server.localIP.ToString(), playerName);
			Benchmark.StopWatches("Start Game");
		}

		public static void Update() {
			Server.Update();
			GameClient.Update();
		}

		public static void Stop() {
			Server.Stop();
			GameClient.Stop();
		}

		static void OnReceive(ButtonMessage message) {
			if (message.button == 0) {
				var volume = world.volumes[0];
				var chunk = volume.ChunkAt(message.blockPosition);
				volume[message.blockPosition] = 0;
				new ChunkMessage(volumeID: 0, chunk).Broadcast();
			}
		}

		static void OnReceive(ConnectClientMessage message) {
			Benchmark.StartWatch("New player chunk messages");
			foreach (var volume in world.volumes) {
				new VolumeMessage(volume.Key).Send(message.connection);
				foreach (var chunk in volume.Value.chunks) {
					new ChunkMessage(volume.Key, chunk).Send(message.connection);
				}
			}
			Benchmark.StopWatches("ConnectClientMessage");
		}

		static void OnReceive(PlaceBlockMessage message) {
			if (any(message.blockPosition < 0) || any(message.blockPosition >= Volume.ChunkDistance * Chunk.Size)) { return; }
			var volume = world.volumes[0];
			var chunk = volume.ChunkAt(message.blockPosition);
			volume[message.blockPosition] = BlockManager.Default("sand").id;
			new ChunkMessage(volumeID: 0, chunk).Broadcast();
		}
	}
}
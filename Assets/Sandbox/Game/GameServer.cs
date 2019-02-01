namespace Sandbox {
	using System.Threading.Tasks;
	using Benchmark;
	using Sandbox.Core;
	using Sandbox.Net;
	using static Unity.Mathematics.math;

	public static class GameServer {
		public static Universe universe = new Universe();
		public static bool running = false;

		public static void Start(string playerName) {
			Message.RegisterServerHandler<ButtonMessage>(OnReceive);
			Message.RegisterServerHandler<ConnectClientMessage>(OnReceive);
			Message.RegisterServerHandler<PlaceBlockMessage>(OnReceive);
			Benchmark.StartWatch("World generation");
			universe.Generate();
			Benchmark.StopWatch("World generation");

			Benchmark.StartWatch("Server start");
			Server.Start(playerName);
			Benchmark.StopWatch("Server start");

			Benchmark.StartWatch("Client start");
			GameClient.Start(Server.localIP.ToString(), playerName);
			Benchmark.StopWatches("Start Game");

			running = true;

			FixedUpdate();
		}

		public static void Update() {
			Server.Update();
			GameClient.Update();
		}

		public static void Stop() {
			Server.Stop();
			GameClient.Stop();
		}

		public static async void FixedUpdate() {
			for (;;) {
				await Task.Delay(50);
				universe.Update();
			}
		}

		static void OnReceive(ButtonMessage message) {
			if (message.button == 0) {
				var volume = universe.volumes[0];
				var chunk = volume.ChunkAt(message.blockPosition);
				volume[message.blockPosition] = 0;
			}
		}

		static void OnReceive(ConnectClientMessage message) {
			foreach (var volume in universe.volumes) {
				new VolumeMessage(volume.Key).Send(message.connection);
				foreach (var chunk in volume.Value.chunks) {
					new ChunkMessage(volume.Key, chunk).Send(message.connection);
				}
			}
		}

		static void OnReceive(PlaceBlockMessage message) {
			if (any(message.blockPosition < 0) || any(message.blockPosition >= Volume.ChunkDistance * Chunk.Size)) { return; }
			var volume = universe.volumes[0];
			var chunk = volume.ChunkAt(message.blockPosition);
			volume[message.blockPosition] = message.id;
		}
	}
}
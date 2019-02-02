namespace Sandbox.Core {
	using System.Threading.Tasks;
	using Sandbox.Net;

	public static class Server {
		public static Universe universe = new Universe();
		public static bool running = false;

		public static void Start(string playerName) {
			Net.Server.Listen<ConnectClientMessage>(SendVolumes);
			Net.Server.Start(playerName);
			running = true;
			Client.Start(Net.Server.localIP.ToString(), playerName);
			FixedUpdate();
		}

		public static void Update() {
			Net.Server.Update();
			Client.Update();
			universe.Update();
		}

		public static void Stop() {
			Net.Server.Stop();
			Client.Stop();
		}

		public static async void FixedUpdate() {
			for (;;) {
				await Task.Delay(50);
				universe.Update();
			}
		}

		static void SendVolumes(ConnectClientMessage message) {
			foreach (var volume in universe.volumes) {
				new VolumeMessage(volume.Key).Send(message.connection);
				foreach (var chunk in volume.Value.chunks) {
					new ChunkMessage(volume.Key, chunk).Send(message.connection);
				}
			}
		}
	}
}
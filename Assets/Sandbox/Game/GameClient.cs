namespace Sandbox {
	using Sandbox.Core;
	using Sandbox.Net;

	public static class GameClient {
		public static Universe universe = new Universe();

		public static void Start(string ip, string playerName) {
			Message.RegisterClientHandler<VolumeMessage>(Universe.OnReceive);
			Message.RegisterClientHandler<ChunkMessage>(Universe.OnReceive);
			Client.Start(ip, playerName);
		}

		public static void Update() {
			Client.Update();
			universe.Update();
		}

		public static void Stop() {
			Client.Stop();
		}

	}
}
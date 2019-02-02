namespace Sandbox.Core {
	using Sandbox.Net;

	public static class Client {
		public static Universe universe = new Universe();

		public static void Start(string ip, string playerName) {
			Message.RegisterClientHandler<VolumeMessage>(universe.OnReceive);
			Message.RegisterClientHandler<ChunkMessage>(universe.OnReceive);
			Net.Client.Start(ip, playerName);
		}

		public static void Update() {
			Net.Client.Update();
		}

		public static void Stop() {
			Net.Client.Stop();
		}

	}
}

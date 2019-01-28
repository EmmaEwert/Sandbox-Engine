namespace Sandbox {
	using Sandbox.Net;

	public static class GameClient {
		public static World world;

		public static void Start(string ip, string playerName) {
			Message.RegisterClientHandler<WorldPartMessage>(World.OnReceive);
			Client.Start(ip, playerName);
		}

		public static void Update() {
			Client.Update();
		}

		public static void Stop() {
			Client.Stop();
		}
	}
}
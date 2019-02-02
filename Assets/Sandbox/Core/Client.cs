namespace Sandbox.Core {
	public static class Client {
		public static Universe universe = new Universe();

		public static void Start(string ip, string playerName) {
			Net.Client.Listen<VolumeMessage>(universe.Add);
			Net.Client.Listen<ChunkMessage>(universe.Update);
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

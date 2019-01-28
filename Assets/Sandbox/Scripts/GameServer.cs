namespace Sandbox {
	using Sandbox.Net;
	using static Unity.Mathematics.math;

	public static class GameServer {
		public static World world;

		public static void Start(string playerName) {
			Message.RegisterServerHandler<ButtonMessage>(OnReceive);
			Message.RegisterServerHandler<ConnectClientMessage>(OnReceive);
			Message.RegisterServerHandler<PlaceBlockMessage>(OnReceive);
			world = new World();
			world.Generate();
			Server.Start(playerName);
			GameClient.Start(Server.localIP.ToString(), playerName);
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
				world.blocks[message.blockPosition.x, message.blockPosition.y, message.blockPosition.z] = 0;
				new WorldPartMessage(world).Broadcast();
			}
		}

		static void OnReceive(ConnectClientMessage message) {
			new WorldPartMessage(world).Send(message.connection);
		}

		static void OnReceive(PlaceBlockMessage message) {
			if (any(message.blockPosition < 0) || any(message.blockPosition >= World.Size)) { return; }
			world.blocks[message.blockPosition.x, message.blockPosition.y, message.blockPosition.z] = 1;
			new WorldPartMessage(world).Broadcast();
		}
	}
}
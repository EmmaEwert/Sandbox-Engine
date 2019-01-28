namespace Sandbox {
	using Sandbox.Net;
	using static Unity.Mathematics.math;

	public static class GameServer {
		public static World world = new World();

		public static void Start(string playerName) {
			Message.RegisterServerHandler<ButtonMessage>(OnReceive);
			Message.RegisterServerHandler<ConnectClientMessage>(OnReceive);
			Message.RegisterServerHandler<PlaceBlockMessage>(OnReceive);
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
				UnityEngine.Debug.Log(message.blockPosition);
				var volume = world.volumes[0];
				var chunk = volume.ChunkAt(message.blockPosition);
				volume[message.blockPosition] = 0;
				new ChunkMessage(volumeID: 0, chunk).Broadcast();
				//world.blocks[message.blockPosition.x, message.blockPosition.y, message.blockPosition.z] = 0;
				// new WorldPartMessage(world).Broadcast();
			}
		}

		static void OnReceive(ConnectClientMessage message) {
			foreach (var volume in world.volumes) {
				new VolumeMessage(volume.Key).Send(message.connection);
				foreach (var chunk in volume.Value.chunks) {
					new ChunkMessage(volume.Key, chunk).Send(message.connection);
				}
			}
		}

		static void OnReceive(PlaceBlockMessage message) {
			if (any(message.blockPosition < 0) || any(message.blockPosition >= Volume.ChunkDistance * Chunk.Size)) { return; }
			var volume = world.volumes[0];
			var chunk = volume.ChunkAt(message.blockPosition);
			volume[message.blockPosition] = 1;
			new ChunkMessage(volumeID: 0, chunk).Broadcast();
			//world.blocks[message.blockPosition.x, message.blockPosition.y, message.blockPosition.z] = 1;
			// new WorldPartMessage(world).Broadcast();
		}
	}
}
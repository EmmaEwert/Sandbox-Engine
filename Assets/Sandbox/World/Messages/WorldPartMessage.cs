namespace Sandbox {
	using Sandbox.Net;

	public class WorldPartMessage : ReliableMessage, IServerMessage {
		public ushort[,,] blocks;

		protected override int length =>
			sizeof(ushort) * World.Size * World.Size * World.Size;

		public WorldPartMessage() { }
		public WorldPartMessage(World world) {
			blocks = new ushort[16, 16, 16];
			for (var z = 0; z < 16; ++z)
			for (var y = 0; y < 16; ++y)
			for (var x = 0; x < 16; ++x) {
				blocks[x, y, z] = world.blocks[x, y, z];
			}
		}

		public void Read(Reader reader) {
			blocks = new ushort[16, 16, 16];
			for (var z = 0; z < 16; ++z)
			for (var y = 0; y < 16; ++y)
			for (var x = 0; x < 16; ++x) {
				reader.Read(out blocks[x, y, z]);
			}
		}

		public void Write(Writer writer) {
			for (var z = 0; z < 16; ++z)
			for (var y = 0; y < 16; ++y)
			for (var x = 0; x < 16; ++x) {
				writer.Write(blocks[x, y, z]);
			}
		}
	}
}
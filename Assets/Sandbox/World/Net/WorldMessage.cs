namespace Sandbox.Net {
	public class WorldPartMessage : Message, IServerMessage {
		public ushort[,,] blocks;
		public ushort offsetX;
		public ushort offsetY;
		public ushort offsetZ;

		protected override int length =>
			sizeof(ushort) * World.Size * World.Size * World.Size
			+ sizeof(byte) * 3;

		public WorldPartMessage() { }

		public WorldPartMessage(World world, ushort offsetX, ushort offsetY, ushort offsetZ) {
			blocks = new ushort[8, 8, 8];
			this.offsetX = offsetX;
			this.offsetY = offsetY;
			this.offsetZ = offsetZ;
			for (var z = offsetZ; z < offsetZ + 8; ++z)
			for (var y = offsetY; y < offsetY + 8; ++y)
			for (var x = offsetX; x < offsetX + 8; ++x) {
				blocks[x % 8, y % 8, z % 8] = world.blocks[x, y, z];
			}
		}

		public void Read(Reader reader) {
			blocks = new ushort[8, 8, 8];
			reader.Read(out offsetX);
			reader.Read(out offsetY);
			reader.Read(out offsetZ);
			for (var z = 0; z < 8; ++z)
			for (var y = 0; y < 8; ++y)
			for (var x = 0; x < 8; ++x) {
				reader.Read(out blocks[x, y, z]);
			}
		}

		public void Write(Writer writer) {
			writer.Write(offsetX);
			writer.Write(offsetY);
			writer.Write(offsetZ);
			for (var z = 0; z < 8; ++z)
			for (var y = 0; y < 8; ++y)
			for (var x = 0; x < 8; ++x) {
				writer.Write(blocks[x, y, z]);
			}
		}
	}
}
namespace Sandbox {
	using Sandbox.Net;
	using System;

	public class ChunkMessage : ReliableMessage, IServerMessage {
		public ushort[,,] blocks;

		protected override int length =>
			sizeof(ushort) * Chunk.Size * Chunk.Size * Chunk.Size;

		public ChunkMessage() { }
		public ChunkMessage(Chunk chunk) {
			blocks = new ushort[Chunk.Size, Chunk.Size, Chunk.Size];
			Buffer.BlockCopy(chunk.blocks, 0, blocks, 0, Chunk.Size * Chunk.Size * Chunk.Size);
		}

		public void Read(Reader reader) {
			blocks = new ushort[Chunk.Size, Chunk.Size, Chunk.Size];
			for (var z = 0; z < Chunk.Size; ++z)
			for (var y = 0; y < Chunk.Size; ++y)
			for (var x = 0; x < Chunk.Size; ++x) {
				reader.Read(out blocks[x, y, z]);
			}
		}

		public void Write(Writer writer) {
			for (var z = 0; z < Chunk.Size; ++z)
			for (var y = 0; y < Chunk.Size; ++y)
			for (var x = 0; x < Chunk.Size; ++x) {
				writer.Write(blocks[x, y, z]);
			}
		}
	}
}

namespace Sandbox {
	using Sandbox.Net;
	using System;
	using Unity.Mathematics;

	public class ChunkMessage : ReliableMessage, IServerMessage {
		public ushort volumeID;
		public int3 pos;
		public ushort[] blocks = new ushort[Chunk.Size * Chunk.Size * Chunk.Size];

		protected override int length =>
			sizeof(ushort)
			+ sizeof(int) * 3
			+ sizeof(ushort) * blocks.Length;

		public ChunkMessage() { }
		public ChunkMessage(ushort volumeID, Chunk chunk) {
			this.volumeID = volumeID;
			this.pos = chunk.pos;
			chunk.ids.CopyTo(blocks);
			//Buffer.BlockCopy(chunk.ids, 0, blocks, 0, blocks.Length * sizeof(ushort));
		}

		public void Read(Reader reader) {
			reader.Read(out volumeID);
			reader.Read(out pos);
			reader.Read(ref blocks);
		}

		public void Write(Writer writer) {
			writer.Write(volumeID);
			writer.Write(pos);
			writer.Write(blocks);
		}
	}
}

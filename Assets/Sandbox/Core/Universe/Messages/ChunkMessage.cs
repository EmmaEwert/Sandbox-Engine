namespace Sandbox.Core {
	using Sandbox.Net;
	using Unity.Mathematics;

	public class ChunkMessage : ReliableMessage, IServerMessage {
		public ushort volumeID;
		public int3 pos;
		public ushort[] ids = new ushort[Chunk.Size * Chunk.Size * Chunk.Size];

		protected override int length =>
			sizeof(ushort)
			+ sizeof(int) * 3
			+ sizeof(ushort) * ids.Length;

		public ChunkMessage() { }
		public ChunkMessage(ushort volumeID, Chunk chunk, bool complete = false) {
			this.volumeID = volumeID;
			this.pos = chunk.pos;
			for (var i = 0; i < chunk.states.Length; ++i) {
				if (complete || (chunk.flags[i] & Chunk.Flag.Dirty) != 0) {
					this.ids[i] = chunk.states[i];
				} else {
					this.ids[i] = 0xffff;
				}
			}
		}

		public void Read(Reader reader) {
			reader.Read(out volumeID);
			reader.Read(out pos);
			reader.Read(ref ids);
		}

		public void Write(Writer writer) {
			writer.Write(volumeID);
			writer.Write(pos);
			writer.Write(ids);
		}
	}
}

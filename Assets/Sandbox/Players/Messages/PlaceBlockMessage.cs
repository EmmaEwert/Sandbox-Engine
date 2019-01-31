namespace Sandbox {
	using Sandbox.Net;
	using Unity.Mathematics;

	public class PlaceBlockMessage : Message, IClientMessage {
		public int3 blockPosition;
		public ushort id;

		protected override int length => sizeof(int) * 3;

		public PlaceBlockMessage() { }
		public PlaceBlockMessage(int3 blockPosition, ushort id) {
			this.blockPosition = blockPosition;
			this.id = id;
		}

		public void Read(Reader reader) {
			reader.Read(out blockPosition);
			reader.Read(out id);
		}

		public void Write(Writer writer) {
			writer.Write(blockPosition);
			writer.Write(id);
		}
	}
}


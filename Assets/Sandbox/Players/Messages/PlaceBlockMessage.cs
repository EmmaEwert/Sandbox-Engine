namespace Sandbox {
	using Sandbox.Net;
	using Unity.Mathematics;

	public class PlaceBlockMessage : Message, IClientMessage {
		public int3 blockPosition;

		protected override int length => sizeof(int) * 3;

		public PlaceBlockMessage() { }
		public PlaceBlockMessage(int3 blockPosition) {
			this.blockPosition = blockPosition;
		}

		public void Read(Reader reader) {
			reader.Read(out blockPosition);
		}

		public void Write(Writer writer) {
			writer.Write(blockPosition);
		}
	}
}


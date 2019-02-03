namespace Sandbox {
	using Sandbox.Core;
	using Sandbox.Net;
	using Unity.Mathematics;

	public class PlaceBlockMessage : Message, IClientMessage {
		public int3 blockPosition;
		public BlockState state;

		protected override int length => sizeof(int) * 3;

		public PlaceBlockMessage() { }
		public PlaceBlockMessage(int3 blockPosition, BlockState state) {
			this.blockPosition = blockPosition;
			this.state = state;
		}

		public void Read(Reader reader) {
			reader.Read(out blockPosition);
			reader.Read(out state.id);
		}

		public void Write(Writer writer) {
			writer.Write(blockPosition);
			writer.Write(state);
		}
	}
}


namespace Sandbox {
	using Sandbox.Net;
	using Unity.Mathematics;

	public class ButtonMessage : Message, IClientMessage {
		public int button;
		public int3 blockPosition;

		protected override int length => sizeof(int) * 4;

		public ButtonMessage() { }
		public ButtonMessage(int button, int3 blockPosition) {
			this.button = button;
			this.blockPosition = blockPosition;
		}

		public void Read(Reader reader) {
			reader.Read(out button);
			reader.Read(out blockPosition);
		}

		public void Write(Writer writer) {
			writer.Write(button);
			writer.Write(blockPosition);
		}
	}
}

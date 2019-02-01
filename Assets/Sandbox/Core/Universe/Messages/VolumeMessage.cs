namespace Sandbox.Core {
	using Sandbox.Net;

	public class VolumeMessage : ReliableMessage, IServerMessage {
		public ushort id;

		protected override int length => sizeof(ushort);

		public VolumeMessage() { }
		public VolumeMessage(ushort id) {
			this.id = id;
		}

		public void Read(Reader reader) {
			reader.Read(out id);
		}

		public void Write(Writer writer) {
			writer.Write(id);
		}
	}
}


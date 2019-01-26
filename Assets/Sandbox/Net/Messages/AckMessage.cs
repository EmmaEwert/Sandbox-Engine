namespace Sandbox.Net {
	internal class AckMessage : Message, IServerMessage {
		internal int id;

		protected override int length => sizeof(int);

		public AckMessage() { }

		internal AckMessage(int connection) {
			this.id = connection;
		}

		public void Read(Reader reader) {
			reader.Read(out id);
		}

		public void Write(Writer writer) {
			writer.Write(id);
		}
	}
}
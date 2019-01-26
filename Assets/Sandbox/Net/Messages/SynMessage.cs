namespace Sandbox.Net {
	using System.Text;

	public class SynMessage : Message, IClientMessage {
		public string name;

		protected override int length =>
			sizeof(int)
			+ Encoding.UTF8.GetByteCount(name);

		public SynMessage() { }

		internal SynMessage(string name) {
			this.name = name;
		}

		public void Read(Reader reader) {
			reader.Read(out name);
			// this.connection = connection
		}

		public void Write(Writer writer) {
			writer.Write(name);
		}
	}
}
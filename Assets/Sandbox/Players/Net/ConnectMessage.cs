namespace Sandbox.Net {
	public class ConnectMessage : Message, IServerMessage {
		public int id;
		public string name;
		public bool local => id == Client.id;

		protected override int length =>
			sizeof(int) // Player ID
			+ sizeof(int) // String length, in bytes
			+ StringSize(name);

		public ConnectMessage() { }

		public ConnectMessage(int id, string name) {
			this.id = id;
			this.name = name;
		}

		public void Read(Reader reader) {
			reader.Read(out id);
			reader.Read(out name);
		}

		public void Write(Writer writer) {
			writer.Write(id);
			writer.Write(name);
		}
	}
}
namespace Sandbox {
	using Sandbox.Net;

	public class ChatMessage : ReliableMessage, IServerMessage, IClientMessage {
		int id;
		public string text;

		public string name => Client.players[id];
		protected override int length => sizeof(int) + StringSize(text);

		public ChatMessage() { }
		public ChatMessage(string text) {
			id = Client.id;
			this.text = text;
		}

		public void Read(Reader reader) {
			reader.Read(out id);
			reader.Read(out text);
		}

		public void Write(Writer writer) {
			writer.Write(id);
			writer.Write(text);
		}
	}
}
namespace Sandbox.Net {
	public class ActualAckMessage : Message, IServerMessage, IClientMessage {
		public int sequence;

		protected override int length => sizeof(int);

		public ActualAckMessage() { }
		public ActualAckMessage(int sequence) {
			this.sequence = sequence;
		}

		public void Read(Reader reader) {
			reader.Read(out sequence);
		}

		public void Write(Writer writer) {
			writer.Write(sequence);
		}
	}
}
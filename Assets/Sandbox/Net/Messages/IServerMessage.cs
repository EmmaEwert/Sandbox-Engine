namespace Sandbox.Net {
	internal interface IServerMessage {
		void Read(Reader reader);
		void Write(Writer writer);
	}
}
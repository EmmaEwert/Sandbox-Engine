namespace Sandbox.Net {
	internal interface IClientMessage {
		void Read(Reader reader);
		void Write(Writer writer);
	}
}

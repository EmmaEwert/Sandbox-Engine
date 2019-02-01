namespace Sandbox {
	using Sandbox.Net;
	using Unity.Mathematics;

	public class ActorTransformMessage : Message, IServerMessage {
		public int id;
		public int type;
		public float3 pos;

		protected override int length => sizeof(int) + sizeof(float) * 3;

		public ActorTransformMessage() { }
		public ActorTransformMessage(int id, int type, float3 pos) {
			this.id = id;
			this.type = type;
			this.pos = pos;
		}

		public void Read(Reader reader) {
			reader.Read(out id);
			reader.Read(out type);
			reader.Read(out pos);
		}

		public void Write(Writer writer) {
			writer.Write(id);
			writer.Write(type);
			writer.Write(pos);
		}
	}
}
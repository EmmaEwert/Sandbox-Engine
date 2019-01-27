namespace Sandbox {
	using Sandbox.Net;
	using Unity.Mathematics;

	public class TransformMessage : Message, IServerMessage, IClientMessage {
		public int id;
		public float3 position;
		public quaternion rotation;

		public bool local => id == Client.id;

		protected override int length =>
			sizeof(int) // Player ID
			+ sizeof(float) * 3 // Position
			+ sizeof(float) * 4; // Rotation

		public TransformMessage() { }
		public TransformMessage(float3 position, quaternion rotation) {
			id = Client.id;
			this.position = position;
			this.rotation = rotation;
		}

		public void Read(Reader reader) {
			reader.Read(out id);
			reader.Read(out position);
			reader.Read(out rotation);
		}

		public void Write(Writer writer) {
			writer.Write(id);
			writer.Write(position);
			writer.Write(rotation);
		}
	}
}
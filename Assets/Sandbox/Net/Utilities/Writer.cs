namespace Sandbox.Net {
	using System;
	using System.IO;
	using System.Text;
	using Unity.Mathematics;

	public class Writer : IDisposable {
		private MemoryStream stream;
		private BinaryWriter writer;

		internal Writer() {
			stream = new MemoryStream();
			writer = new BinaryWriter(stream);
		}

		public void Dispose() {
			stream.Dispose();
			writer.Dispose();
		}

		internal byte[] ToArray() {
			return stream.ToArray();
		}

		internal void Write(int value) => writer.Write(value);
		internal void Write(ushort value) => writer.Write(value);
		internal void Write(float value) => writer.Write(value);
		internal void Write(string value) {
			var bytes = Encoding.UTF8.GetBytes(value);
			writer.Write(bytes.Length);
			writer.Write(bytes);
		}
		internal void Write(int3 value) {
			Write(value.x);
			Write(value.y);
			Write(value.z);
		}
		internal void Write(float3 value) {
			Write(value.x);
			Write(value.y);
			Write(value.z);
		}
		internal void Write(quaternion value) {
			Write(value.value.x);
			Write(value.value.y);
			Write(value.value.z);
			Write(value.value.w);
		}
	}
}
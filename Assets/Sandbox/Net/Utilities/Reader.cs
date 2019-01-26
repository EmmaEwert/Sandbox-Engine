namespace Sandbox.Net {
	using System;
	using System.Text;
	using Unity.Mathematics;
	using Unity.Networking.Transport;
	using static Unity.Mathematics.math;
	using static Unity.Networking.Transport.DataStreamReader;

	public class Reader {
		private DataStreamReader reader;
		private Context context;

		internal Reader(DataStreamReader reader) {
			this.reader = reader;
			this.context = default(Context);
		}

		internal void Read(out ushort value) {
			value = reader.ReadUShort(ref context);
		}

		internal void Read(out int value) {
			value = reader.ReadInt(ref context);
		}

		internal void Read(out string value) {
			var length = reader.ReadInt(ref context);
			var bytes = reader.ReadBytesAsArray(ref context, length);
			value = Encoding.UTF8.GetString(bytes);
		}

		internal void Read(out int3 value) {
			value = int3(
				reader.ReadInt(ref context),
				reader.ReadInt(ref context),
				reader.ReadInt(ref context)
			);
		}

		internal void Read(out float3 value) {
			value = float3(
				reader.ReadFloat(ref context),
				reader.ReadFloat(ref context),
				reader.ReadFloat(ref context)
			);
		}

		internal void Read(out quaternion value) {
			value = quaternion(
				reader.ReadFloat(ref context),
				reader.ReadFloat(ref context),
				reader.ReadFloat(ref context),
				reader.ReadFloat(ref context)
			);
		}

	}
}
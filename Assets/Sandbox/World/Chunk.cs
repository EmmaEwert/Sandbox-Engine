namespace Sandbox {
	public class Chunk {
		public const int Size = 16;

		public ushort[,,] blocks = new ushort[Size, Size, Size];
		public Volume volume;
	}
}
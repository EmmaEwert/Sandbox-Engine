namespace Sandbox.Core {
	public struct BlockState {
		public static implicit operator ushort(BlockState state) => state.id;
		public static implicit operator BlockState(ushort id) => new BlockState { id = id };

		public ushort id;

		public Block block => Block.Find(this);
	}
}
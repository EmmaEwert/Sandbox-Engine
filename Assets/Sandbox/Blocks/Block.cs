namespace Sandbox {
	using System.Collections.Generic;

	public abstract class Block {
		public static List<Block> blocks;

		public Dictionary<string, BlockState> blockStates { get; private set; }
		public BlockState defaultState;
		public string name { get; private set; }
		public virtual bool opaqueCube => true;

		protected Block(string name, string defaultState = "") {
			this.name = name;
			blockStates = BlockManager.Load(name);
			foreach (var blockState in blockStates.Values) {
				blockState.block = this;
			}
			this.defaultState = blockStates[defaultState];
		}

		public enum Face : byte { Down, Up, South, North, West, East }
	}
}
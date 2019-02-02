namespace Sandbox.Core {
	using System;
	using System.Collections.Generic;
	using Unity.Mathematics;

	public abstract class Block {
		public static List<Block> blocks;
		public static Action<Verb, Volume, int3> onDefault;

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

		///<summary>Collision and interaction box</summary>
		public virtual Box Box(Volume volume, int3 pos) =>
			new Box { min = pos, max = pos + 1 };
		
		///<summary>Called on the server when the block changes, or one of its neighbors change.</summary>
		public virtual void OnUpdated(Volume volume, int3 pos) { }

		///<summary>Called on the client at the discretion of user code.</summary>
		public virtual void On(Verb verb, Volume volume, int3 pos) {
			onDefault?.Invoke(verb, volume, pos);
		}

		public enum Face : byte { None, Down, Up, South, North, West, East }

		public abstract class Verb {}
	}
}
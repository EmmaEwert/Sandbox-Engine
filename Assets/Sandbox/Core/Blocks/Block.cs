namespace Sandbox.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Unity.Entities;
	using Unity.Mathematics;

	public abstract class Block {
		public static Action<Verb, Volume, int3> onDefault;
		static Dictionary<Type, Block> typedBlocks = new Dictionary<Type, Block>();

		// BlockStates
		static List<Block> blocks = new List<Block> { null };
		static List<StateModel[]> models = new List<StateModel[]> { null };
		static List<PropertyMap> properties = new List<PropertyMap> { null };

		public static void Initialize() {
			var blockList = Reflector.InstancesOf<Block>();
			foreach (var block in blockList) {
				typedBlocks[block.GetType()] = block;
			}
		}

		public static Block Find<T>() where T : Block {
			return typedBlocks[typeof(T)];
		}

		public static Block Find(BlockState state) {
			try {
				return blocks[state];
			} catch (ArgumentOutOfRangeException e) {
				UnityEngine.Debug.Log(state.id);
				UnityEngine.Debug.Log(e);
				return null;
			}
		}

		public static BlockState FindState(PropertyMap map) {
			var state = (ushort)properties.Select((p, i) => (p, i)).First(pi => map.Equals(pi.p)).i;
			return state;
		}

		public static StateModel Model(BlockState state) {
			return models[state][0];
		}

		public static PropertyMap Properties(BlockState state) {
			return new PropertyMap(properties[state]);
		}

		static BlockState Load(string name, string defaultStateName, Block block) {
			var defaultState = default(BlockState);
			var blockStateFile = new BlockStateFile(name);
			foreach (var variant in blockStateFile.variants) {
				if (variant.Key == defaultStateName) {
					defaultState = (ushort)blocks.Count;
				}
				blocks.Add(block);
				models.Add(new StateModel[variant.Value.models.Length]);
				for (var i = 0; i < models[models.Count - 1].Length; ++i) {
					models[models.Count - 1][i] = new StateModel {
						model = variant.Value.models[i].model,
						x = variant.Value.models[i].x,
						y = variant.Value.models[i].y,
						uvlock = variant.Value.models[i].uvlock,
						weight = variant.Value.models[i].weight
					};
				}
				if (name == "") {
					Block.properties.Add(null);
					continue;
				}
				Block.properties.Add(new PropertyMap(block, variant.Key));
			}
			return defaultState;
		}

		public Dictionary<string, BlockState> blockStates { get; private set; }
		public BlockState defaultBlockState;
		public BlockState defaultState;

		public string name { get; private set; }
		public virtual bool opaqueCube => true;

		public enum Face : byte { None, Down, Up, South, North, West, East }

		protected Block(string name, string defaultState = "") {
			this.name = name;
			this.defaultState = Load(name, defaultState, this);
		}


		///<summary>Collision and interaction box</summary>
		public virtual Box Box(Volume volume, int3 pos) =>
			new Box { min = pos, max = pos + 1 };
		
		///<summary>Called on the server whenever an instance of this block is first created.</summary>
		public virtual Entity CreateEntity(Volume volume, int3 pos) => Entity.Null;
		
		///<summary>Called on the server when the block changes, or one of its neighbors change.</summary>
		public virtual void OnUpdated(Volume volume, int3 pos) { }

		///<summary>Called on the client at the discretion of user code.</summary>
		public virtual void On(Verb verb, Volume volume, int3 pos) {
			onDefault?.Invoke(verb, volume, pos);
		}


		public abstract class Verb {}

		public class StateModel {
			public Model model;
			public int x;
			public int y;
			public bool uvlock;
			public int weight;
		}
	}
}
namespace Sandbox {
	using System.Collections;
	using System.Collections.Generic;
	using Unity.Mathematics;

	public class BlockState : IEnumerable<KeyValuePair<string, Property>> {
		public static List<BlockState> blockStates = new List<BlockState> { null };

		public ushort id;
		public Block block;
		public StateModel[] models;
		public PropertyMap properties { get; private set; }

		public BlockState(string name, BlockManager.BlockStateFile.Variant variant) {
			id = (ushort)blockStates.Count;
			blockStates.Add(this);
			properties = new PropertyMap();
			models = new StateModel[variant.models.Length];
			for (var i = 0; i < models.Length; ++i) {
				models[i] = new StateModel {
					model = variant.models[i].model,
					x = variant.models[i].x,
					y = variant.models[i].y,
					uvlock = variant.models[i].uvlock,
					weight = variant.models[i].weight,
				};
			}
			if (name == "") { return; }
			foreach (var pair in name.Split(',')) {
				var values = pair.Split('=');
				properties[values[0]] = values[1];
			}
		}

		public BlockState(BlockManager.BlockStateFile.Case @case) {
			properties = new PropertyMap();
			//model = @case.models[0].model;
		}

		public StateModel Model(Volume volume, int3 pos) {
			return models[0];
		}

		public StateModel Model() {
			return models[0];
		}

		public IEnumerator<KeyValuePair<string, Property>> GetEnumerator() {
			return properties.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return properties.GetEnumerator();
		}

		public class StateModel {
			public Model model;
			public int x;
			public int y;
			public bool uvlock;
			public int weight;
		}
	}
}

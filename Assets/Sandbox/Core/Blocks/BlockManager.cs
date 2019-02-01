namespace Sandbox.Core {
	using System.Collections.Generic;
	using System.IO;
	using Newtonsoft.Json.Linq;

	public class BlockManager {
		static Dictionary<string, BlockStateFile> variants = new Dictionary<string, BlockStateFile>();
		static Dictionary<string, Dictionary<string, BlockState>> blockStates = 
			new Dictionary<string, Dictionary<string, BlockState>>();
		static Dictionary<string, Block> blocks = new Dictionary<string, Block>();

		public static void Start() {
			Core.Block.blocks = Reflector.InstancesOf<Block>();
			TextureManager.Start();
			ModelManager.Start();
		}

		static Block Block(string name) {
			if (!blocks.TryGetValue(name, out var block)) {
				foreach (var blockState in Load(name).Values) {
					blocks[name] = blockState.block;
					return blocks[name];
				}
			}
			return blocks[name];
		}

		public static Block Block(int id) {
			return BlockState.blockStates[id]?.block;
		}

		public static Dictionary<string, BlockState> Load(string name) {
			if (!blockStates.TryGetValue(name, out var variants)) {
				blockStates[name] = new Dictionary<string, BlockState>();
				var blockStateFile = new BlockStateFile(name);
				foreach (var variant in blockStateFile.variants) {
					blockStates[name].Add(variant.Key, new BlockState(variant.Key, variant.Value));
				}
			}
			return blockStates[name];
		}

		public static BlockState Default(string name) =>
			Block(name).defaultState;

		public class BlockStateFile {
			public Dictionary<string, Variant> variants = new Dictionary<string, Variant>();
			public List<Case> cases = new List<Case>();

			public BlockStateFile(string name) {
				var json = File.ReadAllText($"Resources/blockstates/{name}.json");
				var jsonData = JObject.Parse(json);

				var variants = (JObject)jsonData["variants"];
				if (variants != null) {
					foreach (var variant in variants) {
						this.variants[variant.Key] = new Variant(variant.Value);
					}
					return;
				}

				var multipart = (JArray)jsonData["multipart"];
				if (multipart != null) {
					foreach (var @case in multipart) {
						this.cases.Add(new Case(@case));
					}
				}
			}

			public class Case {
				public (string, string) when;
				public (Model model, int x, int y, bool uvlock, int weight)[] models;

				public Case(JToken @case) {
					var model = @case["apply"];
					if (model.Type == JTokenType.Array) {
						var models = (JArray)model;
						this.models = new (Model, int, int, bool, int)[models.Count];
						for (var i = 0; i < models.Count; ++i) {
							this.models[i].model = ModelManager.Load(models[i]["model"].Value<string>());
							this.models[i].x = (int)(models[i]["x"] ?? 0);
							this.models[i].y = (int)(models[i]["y"] ?? 0);
							this.models[i].uvlock = (bool)(models[i]["uvlock"] ?? false);
							this.models[i].weight = (int)(models[i]["weight"] ?? 1);
						}
					} else {
						var modelObject = (JObject)model;
						this.models = new (Model, int, int, bool, int)[1];
						this.models[0].model = ModelManager.Load(modelObject["model"].Value<string>());
						this.models[0].x = (int)(modelObject["x"] ?? 0);
						this.models[0].y = (int)(modelObject["y"] ?? 0);
						this.models[0].uvlock = (bool)(modelObject["uvlock"] ?? false);
						this.models[0].weight = (int)(modelObject["weight"] ?? 1);
					}
					var when = ((JObject)@case["when"])?.GetEnumerator().Current;
					if (when.HasValue) {
						this.when = (when.Value.Key, when.Value.Value.Value<string>());
					}
				}
			}

			public class Variant {
				public (Model model, int x, int y, bool uvlock, int weight)[] models;

				public Variant(JToken model) {
					if (model.Type == JTokenType.Array) {
						var models = (JArray)model;
						this.models = new (Model, int, int, bool, int)[models.Count];
						for (var i = 0; i < models.Count; ++i) {
							this.models[i].model = ModelManager.Load(models[i]["model"].Value<string>());
							this.models[i].x = (int)(models[i]["x"] ?? 0);
							this.models[i].y = (int)(models[i]["y"] ?? 0);
							this.models[i].uvlock = (bool)(models[i]["uvlock"] ?? false);
							this.models[i].weight = (int)(models[i]["weight"] ?? 1);
						}
					} else {
						var modelObject = (JObject)model;
						this.models = new (Model, int, int, bool, int)[1];
						this.models[0].model = ModelManager.Load(modelObject["model"].Value<string>());
						this.models[0].x = (int)(modelObject["x"] ?? 0);
						this.models[0].y = (int)(modelObject["y"] ?? 0);
						this.models[0].uvlock = (bool)(modelObject["uvlock"] ?? false);
						this.models[0].weight = (int)(modelObject["weight"] ?? 1);
					}
				}
			}
		}
	}
}

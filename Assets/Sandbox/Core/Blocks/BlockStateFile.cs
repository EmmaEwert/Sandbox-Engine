namespace Sandbox.Core {
	using System.Collections.Generic;
	using System.IO;
	using Newtonsoft.Json.Linq;

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

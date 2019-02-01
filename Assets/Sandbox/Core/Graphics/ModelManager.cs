namespace Sandbox.Core {
	using System.Collections.Generic;

	public class ModelManager {
		private static Dictionary<string, Model> models = new Dictionary<string, Model>();

		public static void Start() {
			foreach (var model in models.Values) {
				model.GenerateFaces();
			}
		}

		public static Model Load(string name) {
			if (models.TryGetValue(name, out var model)) {
				return model;
			}
			model = new Model(name);
			models.Add(name, model);
			return model;
		}
	}
}

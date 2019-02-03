namespace Sandbox.Core {
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public class PropertyMap : IEnumerable<KeyValuePair<string, Property>> {
		private Block block;
		private Dictionary<string, Property> properties = new Dictionary<string, Property>();

		public PropertyMap(Block block, string name) {
			this.block = block;
			foreach (var pair in name.Split(',')) {
				var values = pair.Split('=');
				if (values.Length == 2) {
					properties[values[0]] = values[1];
				}
			}
		}

		public PropertyMap(PropertyMap original) {
			this.block = original.block;
			foreach (var pair in original.properties) {
				properties.Add(pair.Key, (string)pair.Value);
			}
		}

		public Property this[string name] {
			get => properties[name];
			set => properties[name] = value;
		}

		public IEnumerator<KeyValuePair<string, Property>> GetEnumerator() =>
			properties.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			properties.GetEnumerator();
		
		public override bool Equals(Object obj) {
			if (obj is PropertyMap map) {
				if (block != map.block) { return false; }
				foreach (var key in properties.Keys) {
					if ((string)properties[key] != (string)map.properties[key]) {
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode() {
			var name = $"{block.GetType().Name}:";
			foreach (var pair in properties) {
				name += $"{pair.Key}={pair.Value},";
			}
			return name.Substring(1).GetHashCode();
		}
	}

	public struct Property {
		private string value;

		public static implicit operator bool(Property property) => bool.Parse(property.value);
		public static implicit operator Property(bool value) => new Property { value = value.ToString().ToLower() };
		public static implicit operator int(Property property) => int.Parse(property.value);
		public static implicit operator Property(int value) => new Property { value = value.ToString() };
		public static implicit operator string(Property property) => property.value;
		public static implicit operator Property(string value) => new Property { value = value };

		public override string ToString() => value;
	}
}

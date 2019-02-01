namespace Sandbox.Core {
	using System.Collections;
	using System.Collections.Generic;

	public class PropertyMap : IEnumerable<KeyValuePair<string, Property>> {
		private SortedList<string, Property> properties = new SortedList<string, Property>();

		public Property this[string name] {
			get => properties[name];
			set => properties[name] = value;
		}

		public IEnumerator<KeyValuePair<string, Property>> GetEnumerator() =>
			properties.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			properties.GetEnumerator();
	}

	public class Property {
		private string value;

		public static implicit operator bool(Property property) => bool.Parse(property.value);
		public static implicit operator Property(bool value) => new Property { value = value.ToString().ToLower() };
		public static implicit operator int(Property property) => int.Parse(property.value);
		public static implicit operator Property(int value) => new Property { value = value.ToString() };
		public static implicit operator string(Property property) => property?.value;
		public static implicit operator Property(string value) => new Property { value = value };

		public override string ToString() => value;
	}
}

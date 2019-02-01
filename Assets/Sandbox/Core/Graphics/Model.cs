namespace Sandbox.Core {
	using static Unity.Mathematics.math;
	using System.Collections.Generic;
	using Newtonsoft.Json.Linq;
	using Unity.Mathematics;
	using System.IO;

	public class Model {
		public string parent;
		public Dictionary<string, string> textureVariables = new Dictionary<string, string>();
		public Element[] elements = new Element[0];
		public Face[] faces;

		public Model(string name) {
			var json = File.ReadAllText($"Resources/models/{name}.json");
			var data = JObject.Parse(json);
			var parent = (string)data["parent"];
			var textures = (JObject)data["textures"];
			var variables = new Dictionary<string, string>();
			var elements = (JArray)data["elements"];
			if (parent != null) {
				var parentModel = ModelManager.Load(parent);
				variables = parentModel.textureVariables;
				this.elements = new Element[parentModel.elements.Length];
				for (var i = 0; i < this.elements.Length; ++i) {
					this.elements[i] = new Element(parentModel.elements[i]);
				}
			}
			if (textures != null) {
				foreach (var texture in textures) {
					variables[texture.Key] = (string)texture.Value;
				}
			}
			if (elements != null) {
				this.elements = new Element[elements.Count];
				for (var i = 0; i < elements.Count; ++i) {
					this.elements[i] = new Element(elements[i]);
				}
			}
			foreach (var key in variables.Keys) {
				var value = variables[key];
				if (value.StartsWith("#") && textureVariables.TryGetValue(value.Substring(1), out var newValue)) {
					textureVariables[key] = newValue;
				} else {
					textureVariables[key] = value;
				}
			}
			foreach (var element in this.elements) {
				foreach (var face in element.faces.Values) {
					var texture = face.texture.Substring(1);
					if (textureVariables.TryGetValue(texture, out var newTexture)) {
						face.texture = newTexture;
					}
				}
			}
			var faceCount = 0;
			foreach (var element in this.elements) {
				foreach (var face in element.faces.Values) {
					++faceCount;
					var texture = face.texture;
					if (!texture.StartsWith("#")) {
						TextureManager.Load(texture);
					}
				}
			}
			faces = new Face[faceCount];
		}

		///<summary>Generates faces with correct UVs based on texture</summary>
		public void GenerateFaces() {
			var faceIndex = 0;
			foreach (var element in this.elements) {
				var from = float3(element.from) / 16f;
				var to = float3(element.to) / 16f;
				foreach (var face in element.faces) {
					var uv = float4(face.Value.uv) / 16f;
					var useUV = any(uv != default(float4));
					var positions = new float3[4];
					var uvs = new float2[4];
					var normal = default(float3);
					var triangles = new int[6];
					var uvOffset = TextureManager.Offset(face.Value.texture);
					switch (face.Key) {
						case "down":
							positions[0] = float3(from.x, from.y, to.z);
							positions[1] = float3(to.x, from.y, from.z);
							positions[2] = float3(to.x, from.y, to.z);
							positions[3] = float3(from.x, from.y, from.z);
							uvs[0] = (useUV ? uv.xy : float2(from.x, from.z)) / TextureManager.texels + uvOffset;
							uvs[1] = (useUV ? uv.zw : float2(to.x, to.z)) / TextureManager.texels + uvOffset;
							uvs[2] = (useUV ? uv.zy : float2(to.x, from.z)) / TextureManager.texels + uvOffset;
							uvs[3] = (useUV ? uv.xw : float2(from.x, to.z)) / TextureManager.texels + uvOffset;
							normal = float3(0, -1, 0);
							triangles = new [] { 0, 1, 2, 0, 3, 1 };
							break;
						case "up":
							positions[0] = float3(from.x, to.y, from.z);
							positions[1] = float3(to.x, to.y, to.z);
							positions[2] = float3(to.x, to.y, from.z);
							positions[3] = float3(from.x, to.y, to.z);
							uvs[0] = (useUV ? uv.xy : float2(from.x, from.z)) / TextureManager.texels + uvOffset;
							uvs[1] = (useUV ? uv.zw : float2(to.x, to.z)) / TextureManager.texels + uvOffset;
							uvs[2] = (useUV ? uv.zy : float2(to.x, from.z)) / TextureManager.texels + uvOffset;
							uvs[3] = (useUV ? uv.xw : float2(from.x, to.z)) / TextureManager.texels + uvOffset;
							normal = float3(0, 1, 0);
							triangles = new [] { 0, 1, 2, 0, 3, 1 };
							break;
						case "north":
							positions[0] = float3(from.x, from.y, from.z);
							positions[1] = float3(to.x, to.y, from.z);
							positions[2] = float3(to.x, from.y, from.z);
							positions[3] = float3(from.x, to.y, from.z);
							uvs[0] = (useUV ? uv.xy : float2(from.x, from.y)) / TextureManager.texels + uvOffset;
							uvs[1] = (useUV ? uv.zw : float2(to.x, to.y)) / TextureManager.texels + uvOffset;
							uvs[2] = (useUV ? uv.zy : float2(to.x, from.y)) / TextureManager.texels + uvOffset;
							uvs[3] = (useUV ? uv.xw : float2(from.x, to.y)) / TextureManager.texels + uvOffset;
							normal = float3(0, 0, -1);
							triangles = new [] { 0, 1, 2, 0, 3, 1 };
							break;
						case "south":
							positions[0] = float3(from.x, from.y, to.z);
							positions[1] = float3(from.x, to.y, to.z);
							positions[2] = float3(to.x, from.y, to.z);
							positions[3] = float3(to.x, to.y, to.z);
							uvs[0] = (useUV ? uv.xy : float2(from.x, from.y)) / TextureManager.texels + uvOffset;
							uvs[1] = (useUV ? uv.xw : float2(from.x, to.y)) / TextureManager.texels + uvOffset;
							uvs[2] = (useUV ? uv.zy : float2(to.x, from.y)) / TextureManager.texels + uvOffset;
							uvs[3] = (useUV ? uv.zw : float2(to.x, to.y)) / TextureManager.texels + uvOffset;
							normal = float3(0, 0, 1);
							triangles = new [] { 0, 2, 3, 0, 3, 1 };
							break;
						case "west":
							positions[0] = float3(from.x, from.y, from.z);
							positions[1] = float3(from.x, to.y, to.z);
							positions[2] = float3(from.x, to.y, from.z);
							positions[3] = float3(from.x, from.y, to.z);
							uvs[0] = (useUV ? uv.xy : float2(from.z, from.y)) / TextureManager.texels + uvOffset;
							uvs[1] = (useUV ? uv.zw : float2(to.z, to.y)) / TextureManager.texels + uvOffset;
							uvs[2] = (useUV ? uv.xw : float2(from.z, to.y)) / TextureManager.texels + uvOffset;
							uvs[3] = (useUV ? uv.zy : float2(to.z, from.y)) / TextureManager.texels + uvOffset;
							normal = float3(-1, 0, 0);
							triangles = new [] { 0, 1, 2, 0, 3, 1 };
							break;
						case "east":
							positions[0] = float3(to.x, from.y, to.z);
							positions[1] = float3(to.x, to.y, from.z);
							positions[2] = float3(to.x, to.y, to.z);
							positions[3] = float3(to.x, from.y, from.z);
							uvs[0] = (useUV ? uv.xy : float2(from.z, from.y)) / TextureManager.texels + uvOffset;
							uvs[1] = (useUV ? uv.zw : float2(to.z, to.y)) / TextureManager.texels + uvOffset;
							uvs[2] = (useUV ? uv.xw : float2(from.z, to.y)) / TextureManager.texels + uvOffset;
							uvs[3] = (useUV ? uv.zy : float2(to.z, from.y)) / TextureManager.texels + uvOffset;
							normal = float3(1, 0, 0);
							triangles = new [] { 0, 1, 2, 0, 3, 1 };
							break;
					}
					faces[faceIndex] = new Face {
						positions = positions,
						uvs = uvs,
						normal = normal,
						triangles = triangles,
						cullface = face.Value.cullface
					};
					++faceIndex;
				}
			}
		}

		public struct Face {
			public float3[] positions;
			public float2[] uvs;
			public float3 normal;
			public int[] triangles;
			public Block.Face cullface;
		}

		public class Element {
			public int3 from;
			public int3 to;
			public Dictionary<string, Face> faces = new Dictionary<string, Face>();

			public Element(JToken element) {
				var from = element["from"];
				this.from = int3((int)from[0], (int)from[1], (int)from[2]);
				var to = element["to"];
				this.to = int3((int)to[0], (int)to[1], (int)to[2]);
				var faces = (JObject)element["faces"];
				foreach (var face in faces) {
					this.faces[face.Key] = new Face(face.Value);
				}
			}

			public Element(Element element) {
				from = element.from;
				to = element.to;
				foreach (var face in element.faces) {
					faces[face.Key] = new Face(face.Value);
				}
			}

			public class Face {
				public string texture;
				public Block.Face cullface;
				public int4 uv;

				public Face(JToken face) {
					texture = (string)face["texture"];
					switch ((string)face["cullface"]) {
						case "down": cullface = Block.Face.Down; break;
						case "up": cullface = Block.Face.Up; break;
						case "north": cullface = Block.Face.South; break;
						case "south": cullface = Block.Face.North; break;
						case "west": cullface = Block.Face.West; break;
						case "east": cullface = Block.Face.East; break;
					}
					var uv = (JArray)face["uv"];
					if (uv != null) {
						this.uv = int4((int)uv[0], (int)uv[1], (int)uv[2], (int)uv[3]);
					}
				}

				public Face(Face face) {
					texture = face.texture;
					cullface = face.cullface;
					uv = face.uv;
				}
			}
		}
	}
}
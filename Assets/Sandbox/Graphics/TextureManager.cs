namespace Sandbox {
	using static Unity.Mathematics.math;
	using UnityEngine;
	using System.Collections.Generic;
	using Unity.Mathematics;
	using System.IO;

	public class TextureManager {
		const int TextureSize = 256;

		static Dictionary<string, int> textureIndices = new Dictionary<string, int>();
		static List<Texture2D> textures = new List<Texture2D>();

		public static int texels { get; private set; }

		///<summary>Generate texture atlas.</summary>
		public static void Start() {
			var size = ceilpow2((int)ceil(sqrt(textures.Count) * TextureSize));
			texels = size / TextureSize;
			var atlas = new Texture2D(size, size, TextureFormat.ARGB32, mipChain: false) {
				anisoLevel = 0,
				filterMode = FilterMode.Point,
			};
			var x = 0;
			var y = 0;
			foreach (var texture in textures) {
				var colors = texture.GetPixels();
				atlas.SetPixels(x, y, TextureSize, TextureSize, colors);
				x += TextureSize;
				if (x >= size) {
					x = 0;
					y += TextureSize;
				}
			}
			atlas.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			WorldManager.BlockMaterial.mainTexture = atlas;
		}

		public static Texture2D Load(string name) {
			if (textureIndices.TryGetValue(name, out var index)) {
				return textures[index];
			}
			textureIndices.Add(name, textures.Count);
			var textureData = File.ReadAllBytes($"Resources/textures/{name}.png");
			var texture = new Texture2D(TextureSize, TextureSize) { filterMode = FilterMode.Point };
			ImageConversion.LoadImage(texture, textureData);
			textures.Add(texture);
			Debug.Log($"Loaded texture Resources/textures/{name}.png…");
			return texture;
		}

		public static float2 Offset(int index) {
			return float2(index % (int)texels, index / (int)texels) / texels;
		}

		public static float2 Offset(string name) {
			return Offset(TextureID(name));
		}

		public static int TextureID(string name) {
			if (textureIndices.TryGetValue(name, out var index)) {
				return index;
			}
			textureIndices.Add(name, textures.Count);
			textures.Add(Resources.Load($"Textures/{name}") as Texture2D);
			return textures.Count - 1;
		}
	}
}
namespace Sandbox {
	using UnityEngine;

	public class WorldManager : MonoBehaviour {
		public static Material BlockMaterial => instance.blockMaterial;
		public static GameObject BlockPrefab => instance.blockPrefab;
		static WorldManager instance;

		public GameObject blockPrefab;
		public Material blockMaterial;

		void Awake() {
			instance = this;
		}
	}
}
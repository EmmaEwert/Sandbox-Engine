namespace Sandbox {
	using UnityEngine;

	public class WorldManager : MonoBehaviour {
		public static GameObject BlockPrefab => instance.blockPrefab;
		static WorldManager instance;

		public GameObject blockPrefab;

		void Awake() {
			instance = this;
		}
	}
}
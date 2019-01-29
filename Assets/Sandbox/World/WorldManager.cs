namespace Sandbox {
	using UnityEngine;

	public class WorldManager : MonoBehaviour {
		public static Material BlockMaterial => instance.blockMaterial;
		static WorldManager instance;

		public Material blockMaterial;

		void Awake() {
			instance = this;
		}
	}
}
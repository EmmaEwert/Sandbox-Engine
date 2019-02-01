namespace Sandbox.Core {
	using UnityEngine;

	public class UniverseManager : MonoBehaviour {
		public static Material BlockMaterial => instance.blockMaterial;
		static UniverseManager instance;

		public Material blockMaterial;

		void Awake() {
			instance = this;
		}
	}
}
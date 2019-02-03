namespace Sandbox.Core {
	using Sandbox.Net;

	public class Common {
		public static void Start() {
			Block.Initialize();
			TextureManager.Start();
			ModelManager.Start();
			ReliableMessage.Start();
		}

		public static void Update() {
			ReliableMessage.Update();
		}
	}
}
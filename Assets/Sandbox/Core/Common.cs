namespace Sandbox.Core {
	using Sandbox.Net;

	public class Common {
		public static void Start() {
			BlockManager.Start();
			ReliableMessage.Start();
		}

		public static void Update() {
			Message.Update();
			ReliableMessage.Update();
		}
	}
}
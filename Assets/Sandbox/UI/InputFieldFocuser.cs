namespace Sandbox {
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(InputField))]
	public class InputFieldFocuser : MonoBehaviour {
		public GameObject background;
		private InputField inputField => GetComponent<InputField>();
		private Image image => GetComponent<Image>();

		void Update() {
			if (Input.GetKeyDown(KeyCode.Return)) {
				image.enabled = true;
				image.color = inputField.colors.highlightedColor;
				inputField.ActivateInputField();
				enabled = false;
				background.SetActive(true);
				if (PlayerController.instance) {
					PlayerController.instance.enabled = false;
				}
			} else if (PlayerController.instance) {
				PlayerController.instance.enabled = true;
			}
		}
	}
}
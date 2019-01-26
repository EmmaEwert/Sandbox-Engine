namespace Sandbox {
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(InputField))]
	public class InputFieldFocuser : MonoBehaviour {
		private InputField inputField => GetComponent<InputField>();
		private Image image => GetComponent<Image>();

		void Update() {
			if (Input.GetKeyDown(KeyCode.Return)) {
				image.enabled = true;
				image.color = inputField.colors.highlightedColor;
				inputField.ActivateInputField();
				enabled = false;
				if (CharacterController.instance) {
					CharacterController.instance.enabled = false;
				}
			} else if (CharacterController.instance) {
				CharacterController.instance.enabled = true;
			}
		}
	}
}
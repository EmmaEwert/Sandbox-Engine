namespace Sandbox {
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(InputField))]
	public class PlayerNameReader : MonoBehaviour {
		InputField inputField => GetComponent<InputField>();

		void Start() {
			inputField.placeholder.GetComponent<Text>().text = PlayerPrefs.GetString("name", "Emma");
		}
	}
}
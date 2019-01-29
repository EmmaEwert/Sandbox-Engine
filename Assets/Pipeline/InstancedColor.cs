using UnityEngine;

public class InstancedColor : MonoBehaviour {
	private static int colorID = Shader.PropertyToID("_Color");
	private static MaterialPropertyBlock _propertyBlock;
	private static MaterialPropertyBlock propertyBlock =>
		_propertyBlock = _propertyBlock ?? new MaterialPropertyBlock();

	[SerializeField] private Color color = Color.white;

	private void Awake() {
		OnValidate();
	}

	private void OnValidate() {
		var propertyBlock = new MaterialPropertyBlock();
		propertyBlock.SetColor(colorID, color);
		GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
	}
}
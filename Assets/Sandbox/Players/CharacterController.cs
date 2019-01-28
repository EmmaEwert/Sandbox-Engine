namespace Sandbox {
	using Sandbox.Net;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class CharacterController : MonoBehaviour {
		public static CharacterController instance;
		public Transform pivot;
		public new Transform camera;
		public Transform lineBox => GameObject.Find("Line Box").transform;
		float speed = 4f;

		void Awake() {
			instance = this;
		}

		void OnEnable() {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		void OnDisable() {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		void Update() {
			// Aiming
			var aim = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
			pivot.Rotate(0f, aim.x, 0f);
			camera.Rotate(-aim.y, 0f, 0f);

			// Walking
			var movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			movement = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f) * movement;
			transform.Translate(movement * speed * Time.deltaTime);

			// Looking
			if (Physics.Raycast(camera.position, camera.forward, out var hit, 8f)) {
				lineBox.position = hit.transform.position;
				if (Input.GetButtonDown("Fire1")) {
					new ButtonMessage(0, int3(round(hit.transform.parent.localPosition + hit.transform.localPosition))).Send();
				}
				if (Input.GetButtonDown("Fire2")) {
					new PlaceBlockMessage(int3(round(hit.transform.parent.localPosition + hit.transform.localPosition + hit.normal))).Send();
				}
			}

			if (any(float3(movement) != 0) || any(float2(aim) != 0)) {
				new TransformMessage(transform.position, pivot.rotation).Send();
			}
		}
	}
}
namespace Sandbox {
	using Sandbox.Net;
	using Unity.Mathematics;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class CharacterController : MonoBehaviour {
		public static CharacterController instance;
		public Transform pivot;
		public new Transform camera;
		public Transform lineBox => GameObject.Find("Line Box").transform;
		float speed = 4f;
		float3 velocity;

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
			var volume = GameClient.world.volumes[0];

			// Aiming
			var aim = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
			pivot.Rotate(0f, aim.x, 0f);
			camera.Rotate(-aim.y, 0f, 0f);
			
			// Gravity
			var gravity = float3(0, -20f, 0) * Time.deltaTime;
			var newBox = new Box {
				min = float3(transform.position) - float3(0.25f, 0.00f, 0.25f) + velocity.y * Time.deltaTime,
				max = float3(transform.position) + float3(0.25f, 0.75f, 0.25f) + velocity.y * Time.deltaTime
			};
			if (Physics.Intersects(volume, newBox)) {
				velocity.y = 0;
			} else {
				velocity += gravity;
				velocity = Vector3.ClampMagnitude(velocity, 16f);
			}

			// Jumping
			if (velocity.y == 0 && Input.GetButton("Jump")) {
				velocity.y = 8f;
			}
			transform.Translate(velocity * Time.deltaTime, Space.World);

			// Walking
			var movement = float3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			movement = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f) * movement;
			var newNewBox = new Box {
				min = float3(transform.position) - float3(0.25f, -0.5f, 0.25f) + movement * speed * Time.deltaTime,
				max = float3(transform.position) + float3(0.25f,  0.5f, 0.25f) + movement * speed * Time.deltaTime
			};
			if (Physics.Intersects(volume, newNewBox)) {
				movement = float3(0);
			}
			transform.Translate(movement * speed * Time.deltaTime);

			// Looking
			var ray = new Ray(camera.position, camera.forward);
			if (Physics.Intersects(volume, ray, out var hit, maxDistance: 5)) {
				lineBox.position = float3(volume.gameObject.transform.position) + float3(hit.position);
				if (Input.GetButtonDown("Fire1")) {
					new ButtonMessage(0, hit.position).Send();
				}
				if (Input.GetButtonDown("Fire2")) {
					new PlaceBlockMessage(hit.position + int3(hit.normal)).Send();
				}
			} else {
				lineBox.position = float3(0, 0, -1000);
			}

			if (any(velocity != 0) || any(float3(movement) != 0) || any(float2(aim) != 0)) {
				new TransformMessage(transform.position, pivot.rotation).Send();
			}
		}

		void OnDrawGizmos() {
			Gizmos.DrawCube(float3(transform.position) + float3(0, 1, 0), float3(0.5f, 2f, 0.5f));
		}
	}
}
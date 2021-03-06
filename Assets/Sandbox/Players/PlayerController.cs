﻿namespace Sandbox {
	using Sandbox.Core;
	using Unity.Mathematics;
	using UnityEngine;
	using UnityEngine.UI;
	using static Unity.Mathematics.math;

	public class PlayerController : MonoBehaviour {
		public static PlayerController instance;
		public Transform pivot;
		public new Transform camera;
		public Transform lineBox => GameObject.Find("Line Box").transform;
		float speed = 4f;
		float3 velocity;
		BlockState state;

		void PlaceOrRemoveBlock(Block.Verb verb, Volume volume, int3 pos) {
			if (verb is Push push) {
				volume[pos + push.normal] = push.state;
			} else if (verb is Pull) {
				volume[pos] = 0;
			}
		}

		void Awake() {
			instance = this;
			state = Block.Find<Sand>().defaultState;
			Block.onDefault = PlaceOrRemoveBlock;
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
			var volume = Client.universe.volumes[0];

			// Block selection
			if (Input.GetKeyDown(KeyCode.Alpha1)) {
				state = Block.Find<Sand>().defaultState;
			} else if (Input.GetKeyDown(KeyCode.Alpha2)) {
				state = Block.Find<Cobblestone>().defaultState;
			} else if (Input.GetKeyDown(KeyCode.Alpha3)) {
				state = Block.Find<Stone>().defaultState;
			} else if (Input.GetKeyDown(KeyCode.Alpha4)) {
				state = Block.Find<Dirt>().defaultState;
			} else if (Input.GetKeyDown(KeyCode.Alpha5)) {
				state = Block.Find<Lever>().defaultState;
			}
			GameObject.Find("Selected Block").GetComponent<Text>().text =
				state.block.GetType().Name;

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
			if (Core.Physics.Intersects(volume, newBox)) {
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
			if (Core.Physics.Intersects(volume, newNewBox)) {
				movement = float3(0);
			}
			transform.Translate(movement * speed * Time.deltaTime);

			// Looking
			var ray = new Core.Ray(camera.position, camera.forward);
			if (Core.Physics.Intersects(volume, ray, out var hit, maxDistance: 5)) {
				var box = volume[hit.position].block.Box(volume, hit.position);
				lineBox.position = float3(volume.gameObject.transform.position) + box.min;
				lineBox.localScale = box.max - box.min;
				var block = volume[hit.position].block;
				if (Input.GetButtonDown("Fire1")) {
					block.On(new Pull(), volume, hit.position);
				}
				if (Input.GetButtonDown("Fire2")) {
					block.On(new Push { state = state, normal = new int3(hit.normal) }, volume, hit.position);
				}
				if (Input.GetKeyDown(KeyCode.E)) {
					block.On(new Interact(), volume, hit.position);
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
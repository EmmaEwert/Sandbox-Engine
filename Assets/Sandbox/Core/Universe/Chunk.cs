namespace Sandbox.Core {
	using System;
	using System.Collections.Generic;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public class Chunk {
		public const int Size = 16;
		static int3 PosToBlockIndex = int3(1, Size, Size * Size);

		public int3 pos;
		public GameObject gameObject;
		public bool dirty;
		public BlockState[] states { get; private set; } = new BlockState[Size * Size * Size];
		BlockState[] nextStates = new BlockState[Size * Size * Size];
		NativeArray<BlockState> generatedIDs;
		public Flag[] flags = new Flag[Size * Size * Size];
		Volume volume;
		Dictionary<int3, Entity> entities = new Dictionary<int3, Entity>();

		public Chunk(int3 pos, Volume volume) {
			this.pos = pos;
			this.volume = volume;
		}

		///<summary>Get or set blockstate ID at position. Flags for update if needed.</summary>
		public BlockState this[int3 pos] {
			get {
				var index = dot(PosToBlockIndex, pos);
				var state = states[index];
				return state != 0 ? state : nextStates[index];
			}
			set {
				var index = dot(PosToBlockIndex, pos);
				if (states[index] == value) { return; }
				nextStates[index] = value;
				flags[dot(PosToBlockIndex, pos)] |= Flag.Updated;
				dirty = true;
				volume.dirty = true;
				if (volume.server && value.block != states[index].block) {
					var manager = World.Active.GetOrCreateManager<EntityManager>();
					if (entities.TryGetValue(pos, out var entity)) {
						manager.DestroyEntity(entity);
						entities.Remove(pos);
					}
					entity = value.block?.CreateEntity(volume, pos + this.pos) ?? Entity.Null;
					if (entity != Entity.Null) {
						entities[pos] = entity;
					}
				}
			}
		}

		///<summary>Copy new IDs to current array, flag neighbors of updated blocks for updates.</summary>
		public void PreUpdate(Volume volume) {
			for (var i = 0; i < nextStates.Length; ++i) {
				if (states[i] != nextStates[i]) {
					states[i] = nextStates[i];
					flags[i] |= Flag.Dirty;
				}
			}
			for (var i = 0; i < states.Length; ++i) {
				if ((flags[i] & Flag.Updated) == 0) { continue; }
				var pos = this.pos + i / PosToBlockIndex % Size;
				volume.Set(pos + int3(-1,  0,  0), Flag.NeighborChanged);
				volume.Set(pos + int3( 0, -1,  0), Flag.NeighborChanged);
				volume.Set(pos + int3( 0,  0, -1), Flag.NeighborChanged);
				volume.Set(pos + int3( 1,  0,  0), Flag.NeighborChanged);
				volume.Set(pos + int3( 0,  1,  0), Flag.NeighborChanged);
				volume.Set(pos + int3( 0,  0,  1), Flag.NeighborChanged);
			}
		}

		///<summary>Update all non-empty blocks that changed or had their neighbors change.</summary>
		public void Update(Volume volume) {
			dirty = false;
			for (var i = 0; i < states.Length; ++i) {
				if (states[i] != 0 && (flags[i] & (Flag.Updated | Flag.NeighborChanged)) != 0) {
					flags[i] &= ~(Flag.Updated | Flag.NeighborChanged);
					var pos = this.pos + i / PosToBlockIndex % Size;
					if (volume.server) {
						states[i].block.OnUpdated(volume, pos);
					}
				} else {
					flags[i] &= ~(Flag.Updated | Flag.NeighborChanged);
				}
			}
		}

		public void Clean() {
			for (var i = 0; i < states.Length; ++i) {
				flags[i] &= ~Flag.Dirty;
			}
		}

		public void ClientUpdate(Volume volume) {
			dirty = false;
			for (var i = 0; i < states.Length; ++i) {
				if ((flags[i] & Flag.Updated) != 0) {
					var pos = this.pos + i / PosToBlockIndex % Size;
					new PlaceBlockMessage(pos, states[i]).Send();
				}
			}
			//Update(volume);
			UpdateGeometry(volume);
		}

		public void Set(int3 pos, Flag flag) {
			flags[dot(PosToBlockIndex, pos)] |= flag;
			dirty = true;
		}

		public void IDs(ushort[] ids) {
			var changed = false;
			for (var i = 0; i < ids.Length; ++i) {
				if (ids[i] != 0xffff) {
					if (this.nextStates[i] != ids[i]) {
						changed = true;
						this.states[i] = nextStates[i] = ids[i];
					}
				}
			}
			if (changed) {
				UpdateGeometry(volume);
			}
		}

		public JobHandle Generate() {
			generatedIDs = new NativeArray<BlockState>(this.states, Allocator.TempJob);
			return new GenerateJob {
				pos = pos,
				dirt = Block.Find<Dirt>().defaultState,
				stone = Block.Find<Stone>().defaultState,
				ids = generatedIDs,
			}.Schedule(states.Length, Size);
		}

		public void AssignGeneratedIDs() {
			generatedIDs.CopyTo(states);
			generatedIDs.CopyTo(nextStates);
			generatedIDs.Dispose();
		}

		///<summary>Regenerates mesh for the chunk.</summary>
		public void UpdateGeometry(Volume volume) {
			var opaqueBlocks = new bool[Size * Size * Size];
			for (var z = 0; z < Size; ++z)
			for (var y = 0; y < Size; ++y)
			for (var x = 0; x < Size; ++x) {
				var neighbor = this[int3(x, y, z)];
				opaqueBlocks[x + y * Size + z * Size * Size] =
					neighbor != 0 && Block.Find(neighbor).opaqueCube;
			}

			var vertices = new List<Vector3>();
			var uvs = new List<Vector2>();
			var normals = new List<Vector3>();
			var triangles = new List<int>();
			for (var pos = int3(0); pos.z < Size; ++pos.z)
			for (pos.y = 0; pos.y < Size; ++pos.y)
			for (pos.x = 0; pos.x < Size; ++pos.x) {
				var state = this[pos];
				if (state == 0) { continue; }
				var index = dot(PosToBlockIndex, pos);
				var stateModel = Block.Model(state);
				var faces = stateModel.model.faces;
				for (var i = 0; i < faces.Length; ++i) {
					var face = faces[i];
					if (face.cullface != Block.Face.None) {
						var faceNormal = BlockUtility.Normals[(byte)face.cullface];
						var neighborPos = pos + faceNormal;
						if (all(neighborPos >= 0) && all(neighborPos < Size)) {
							if (opaqueBlocks[dot(neighborPos, PosToBlockIndex)]) {
								continue;
							}
						}
					}
					for (var j = 0; j < 6; ++j) {
						triangles.Add(face.triangles[j] + vertices.Count);
					}
					for (var j = 0; j < 4; ++j) {
						vertices.Add(Math.RotateAroundPivot(face.positions[j], float3(0.5f), float3(stateModel.x, stateModel.y, 0f)) + pos);
						uvs.Add(face.uvs[j]); // TODO: uvlock
						normals.Add(face.normal);
					}
				}
			}
			var mesh = new Mesh();
			mesh.SetVertices(vertices);
			mesh.SetNormals(normals);
			mesh.SetUVs(0, uvs);
			mesh.SetTriangles(triangles, 0);
			gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
			mesh.UploadMeshData(markNoLongerReadable: true);
		}

		public enum Flag : byte {
			None            = 0b00000000,
			Updated         = 0b00000001,
			NeighborChanged = 0b00000010,
			Dirty           = 0b00000100, // Indicates they should be sent over the network
		}

		[BurstCompile]
		struct GenerateJob : IJobParallelFor {
			[ReadOnly] public int3 pos;
			[ReadOnly] public BlockState dirt;
			[ReadOnly] public BlockState stone;
			[WriteOnly] public NativeArray<BlockState> ids;

			public void Execute(int index) {
				var x = pos.x + index % Size;
				var y = pos.y + (index / Size) % Size;
				var z = pos.z + (index / Size / Size) % Size;
				if (y < Chunk.Size * Volume.ChunkDistance / 2) {
					var noise = Mathf.PerlinNoise(x / 30f, z / 30f);
					ids[index] = (ushort)(noise > 0.5f ? dirt : stone);
				}
			}
		}
	}
}
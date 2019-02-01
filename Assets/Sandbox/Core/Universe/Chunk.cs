namespace Sandbox.Core {
	using System;
	using System.Collections.Generic;
	using Unity.Burst;
	using Unity.Collections;
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
		public ushort[] ids { get; private set; } = new ushort[Size * Size * Size];
		ushort[] nextIDs = new ushort[Size * Size * Size];
		NativeArray<ushort> generatedIDs;
		Flag[] flags = new Flag[Size * Size * Size];
		Volume volume;

		public Chunk(int3 pos, Volume volume) {
			this.pos = pos;
			this.volume = volume;
		}

		///<summary>Get or set blockstate ID at position. Flags for update if needed.</summary>
		public ushort this[int3 pos] {
			get {
				var index = dot(PosToBlockIndex, pos);
				var id = ids[index];
				return id != 0 ? id : nextIDs[index];
			}
			set {
				var index = dot(PosToBlockIndex, pos);
				if (ids[index] == value) { return; }
				nextIDs[index] = value;
				flags[dot(PosToBlockIndex, pos)] |= Flag.Updated;
				dirty = true;
			}
		}

		///<summary>Copy new IDs to current array, flag neighbors of updated blocks for updates.</summary>
		public void PreUpdate(Volume volume) {
			Buffer.BlockCopy(nextIDs, 0, ids, 0, ids.Length * sizeof(ushort));
			for (var i = 0; i < ids.Length; ++i) {
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
			for (var i = 0; i < ids.Length; ++i) {
				if (ids[i] != 0 && (flags[i] & (Flag.Updated | Flag.NeighborChanged)) != 0) {
					flags[i] &= ~(Flag.Updated | Flag.NeighborChanged);
					var pos = this.pos + i / PosToBlockIndex % Size;
					BlockManager.Block(ids[i]).OnUpdated(volume, pos);
				} else {
					flags[i] &= ~(Flag.Updated | Flag.NeighborChanged);
				}
			}
		}

		public void Set(int3 pos, Flag flag) {
			flags[dot(PosToBlockIndex, pos)] |= flag;
			dirty = true;
		}

		public void IDs(ushort[] ids) {
			this.ids = ids;
			Buffer.BlockCopy(ids, 0, nextIDs, 0, ids.Length * sizeof(ushort));
		}

		public JobHandle Generate() {
			generatedIDs = new NativeArray<ushort>(this.ids, Allocator.TempJob);
			return new GenerateJob {
				pos = pos,
				dirt = BlockManager.Default("dirt").id,
				stone = BlockManager.Default("stone").id,
				ids = generatedIDs,
			}.Schedule(ids.Length, Size);
		}

		public void AssignGeneratedIDs() {
			generatedIDs.CopyTo(ids);
			generatedIDs.CopyTo(nextIDs);
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
					neighbor != 0 && BlockState.blockStates[neighbor].block.opaqueCube;
			}

			var vertices = new List<Vector3>();
			var uvs = new List<Vector2>();
			var normals = new List<Vector3>();
			var triangles = new List<int>();
			for (var pos = int3(0); pos.z < Size; ++pos.z)
			for (pos.y = 0; pos.y < Size; ++pos.y)
			for (pos.x = 0; pos.x < Size; ++pos.x) {
				var stateID = this[pos];
				if (stateID == 0) { continue; }
				var index = dot(PosToBlockIndex, pos);
				var state = BlockState.blockStates[stateID];
				var stateModel = state.models[0];
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
		}

		[BurstCompile]
		struct GenerateJob : IJobParallelFor {
			[ReadOnly] public int3 pos;
			[ReadOnly] public ushort dirt;
			[ReadOnly] public ushort stone;
			[WriteOnly] public NativeArray<ushort> ids;

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
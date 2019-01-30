namespace Sandbox {
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
		static int3[] faceNormals = new [] {
			int3( 0,  0,  0),
			int3( 0, -1,  0),
			int3( 0,  1,  0),
			int3( 0,  0, -1),
			int3( 0,  0,  1),
			int3(-1,  0,  0),
			int3( 1,  0,  0)
		};
		static Block.Face[] faceValues = new [] {
			Block.Face.None,
			Block.Face.Down,
			Block.Face.Up,
			Block.Face.South,
			Block.Face.North,
			Block.Face.West,
			Block.Face.East
		};

		public ushort[] ids = new ushort[Size * Size * Size];
		public int3 pos;
		public GameObject gameObject;
		public bool dirty;
		NativeArray<ushort> generatedIDs;
		Flag[] flags = new Flag[Size * Size * Size];

		public Chunk(int3 pos) {
			this.pos = pos;
		}

		public ushort this[int3 pos] {
			get => ids[dot(pos, PosToBlockIndex)];
			set {
				var index = dot(pos, PosToBlockIndex);
				dirty = ids[index] != value;
				flags[index] |= dirty ? Flag.Dirty : Flag.None;
				if (pos.x % Size > 0) { flags[dot(pos + int3(-1,  0,  0), PosToBlockIndex)] |= dirty ? Flag.Dirty : Flag.None; }
				if (pos.y % Size > 0) { flags[dot(pos + int3( 0, -1,  0), PosToBlockIndex)] |= dirty ? Flag.Dirty : Flag.None; }
				if (pos.z % Size > 0) { flags[dot(pos + int3( 0,  0, -1), PosToBlockIndex)] |= dirty ? Flag.Dirty : Flag.None; }
				if (pos.x % Size < Size - 1) { flags[dot(pos + int3( 1,  0,  0), PosToBlockIndex)] |= dirty ? Flag.Dirty : Flag.None; }
				if (pos.y % Size < Size - 1) { flags[dot(pos + int3( 0,  1,  0), PosToBlockIndex)] |= dirty ? Flag.Dirty : Flag.None; }
				if (pos.z % Size < Size - 1) { flags[dot(pos + int3( 0,  0,  1), PosToBlockIndex)] |= dirty ? Flag.Dirty : Flag.None; }
				ids[index] = value;
			}
		}

		public void Update(Volume volume) {
			if (!dirty) { return; }
			for (var i = 0; i < ids.Length; ++i) {
				if ((flags[i] & Flag.Dirty) != 0) {
					flags[i] &= ~Flag.Dirty;
					if (ids[i] == 0) { continue; }
					var pos = this.pos + int3(i % Size, i / Size % Size, i / Size / Size);
					BlockState.blockStates[ids[i]].block.OnPlaced(volume, pos);
				}
			}
			dirty = false;
			for (var i = 0; i < ids.Length; ++i) {
				if ((flags[i] & Flag.Dirty) != 0) {
					dirty = true;
					return;
				}
			}
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
				var index = dot(pos, PosToBlockIndex);
				var state = BlockState.blockStates[stateID];
				var stateModel = state.models[0];
				var faces = stateModel.model.faces;
				for (var i = 0; i < faces.Length; ++i) {
					var face = faces[i];
					var faceIndex = (byte)face.cullface;
					var faceMask = faceValues[faceIndex];
					if (face.cullface == faceMask) {
						var faceNormal = Chunk.faceNormals[faceIndex];
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
						vertices.Add(RotateAroundPivot(face.positions[j], float3(0.5f), float3(stateModel.x, stateModel.y, 0f)) + pos);
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

		static float3 RotateAroundPivot(float3 point, float3 pivot, float3 angles) {
			return float3(Quaternion.Euler(angles) * (point - pivot)) + pivot;
		}

		public enum Flag : byte {
			None = 0b00000000,
			Dirty = 0b00000001,
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
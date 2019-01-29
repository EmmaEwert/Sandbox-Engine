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

		public NativeArray<ushort> ids;
		public int3 pos;
		public GameObject gameObject;

		public Chunk(int3 pos) {
			this.ids = new NativeArray<ushort>(Size * Size * Size, Allocator.Persistent);
			this.pos = pos;
		}

		~Chunk() {
			ids.Dispose();
		}

		public ushort this[int3 pos] {
			get => ids[dot(pos, PosToBlockIndex)];
			set => ids[dot(pos, PosToBlockIndex)] = value;
		}

		public JobHandle Generate() {
			return new GenerateJob {
				pos = pos,
				ids = this.ids
			}.Schedule(ids.Length, Size);
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

		[BurstCompile]
		struct GenerateJob : IJobParallelFor {
			[ReadOnly] public int3 pos;
			[WriteOnly] public NativeArray<ushort> ids;

			public void Execute(int index) {
				var x = pos.x + index % Size;
				var y = pos.y + (index / Size) % Size;
				var z = pos.z + (index / Size / Size) % Size;
				if (y < Chunk.Size * Volume.ChunkDistance / 2) {
					var noise = Mathf.PerlinNoise(x / 30f, z / 30f);
					ids[index] = (ushort)(noise > 0.5f ? 3 : 2); // BlockManager.Default("sand").id;
				}
			}
		}
	}
}
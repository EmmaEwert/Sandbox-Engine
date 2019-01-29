namespace Sandbox {
	using System.Collections.Generic;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine;
	using static Unity.Mathematics.math;
	using static Unity.Mathematics.math_2;

	public class Chunk {
		public const int Size = 16;
		static int3 PosToBlockIndex = int3(1, Size, Size * Size);
		static (int3 normal, byte mask)[] faceNormals = new (int3, byte)[] {
			(int3( 0, -1,  0), 1 << (byte)Block.Face.Down),
			(int3( 0,  1,  0), 1 << (byte)Block.Face.Up),
			(int3( 0,  0, -1), 1 << (byte)Block.Face.South),
			(int3( 0,  0,  1), 1 << (byte)Block.Face.North),
			(int3(-1,  0,  0), 1 << (byte)Block.Face.West),
			(int3( 1,  0,  0), 1 << (byte)Block.Face.East),
		};

		public ushort[] ids = new ushort[Size * Size * Size];
		public int3 pos;
		public GameObject gameObject;

		public Chunk(int3 pos) {
			this.pos = pos;
		}

		public ushort this[int3 pos] {
			get => ids[dot(pos, PosToBlockIndex)];
			set => ids[dot(pos, PosToBlockIndex)] = value;
		}

		///<summary>Regenerates submeshes for each material in the chunk.</summary>
		public void UpdateGeometry(Volume volume) {
			var vertices = new List<Vector3>();
			var uvs = new List<Vector2>();
			var normals = new List<Vector3>();
			var triangles = new List<int>();
			for (var z = 0; z < Size; ++z)
			for (var y = 0; y < Size; ++y)
			for (var x = 0; x < Size; ++x) {
				var stateID = this[int3(x, y, z)];
				if (stateID == 0) { continue; }
				var index = x + y * Size + z * Size * Size;
				var state = BlockState.blockStates[stateID];
				var stateModel = state.Model(volume, pos + int3(x, y, z));
				var faces = stateModel.model.faces;
				var vertexOffset = float3(x, y, z);
				for (var i = 0; i < faces.Length; ++i) {
					var face = faces[i];
					var faceNormal = faceNormals[log2_floor(face.cullface)];
					if ((face.cullface & faceNormal.mask) != 0) {
						var neighbor = volume[int3(x, y, z) + this.pos + faceNormal.normal];
						if (neighbor != 0 && BlockState.blockStates[neighbor].block.opaqueCube) {
							continue;
						}
					}
					for (var j = 0; j < 6; ++j) {
						triangles.Add(face.triangles[j] + vertices.Count);
					}
					for (var j = 0; j < 4; ++j) {
						vertices.Add(RotateAroundPivot(face.positions[j], float3(0.5f), float3(stateModel.x, stateModel.y, 0f)) + vertexOffset);
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
			return;
		}

		static float3 RotateAroundPivot(float3 point, float3 pivot, float3 angles) {
			return float3(Quaternion.Euler(angles) * (point - pivot)) + pivot;
		}
	}
}
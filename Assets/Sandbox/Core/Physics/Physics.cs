namespace Sandbox.Core {
	using Unity.Mathematics;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public static class Physics {
		public static bool Intersects(Volume volume, Box box) {
			box.min = -float3(volume.position) + box.min;
			box.max = -float3(volume.position) + box.max;
			var volumeBox = new Box {
				min = floor(box.min) - 1,
				max = ceil(box.max) + 1,
			};
			for (var z = (int)volumeBox.min.z; z <= volumeBox.max.z; ++z)
			for (var y = (int)volumeBox.min.y; y <= volumeBox.max.y; ++y)
			for (var x = (int)volumeBox.min.x; x <= volumeBox.max.x; ++x) {
				var state = volume[int3(x, y, z)];
				if (state == 0) { continue; }
				var blockBox = Block.Find(state).Box(volume, int3(x, y, z));
				if (Intersects(box, blockBox)) {
					return true;
				}
			}
			return false;
		}

		public static bool Intersects(Volume volume, Ray ray, out Hit hit, float maxDistance = Mathf.Infinity) {
			// TODO: Optimise to only look for hits in the direction of the ray.
			ray.origin = -float3(volume.position) + ray.origin;
			var volumeBox = new Box {
				min = max(min(ray.origin, ray.origin + ray.direction * maxDistance) - 1, -float3(volume.gameObject.transform.position) - Volume.SimDistance),
				max = min(max(ray.origin, ray.origin + ray.direction * maxDistance) + 1, -float3(volume.gameObject.transform.position) + Volume.SimDistance),
			};
			var shortestDistanceSqr = Mathf.Infinity;
			hit = new Hit();
			hit.id = 0;
			for (var z = (int)volumeBox.min.z; z <= volumeBox.max.z; ++z)
			for (var y = (int)volumeBox.min.y; y <= volumeBox.max.y; ++y)
			for (var x = (int)volumeBox.min.x; x <= volumeBox.max.x; ++x) {
				var difference = ray.origin - (float3(x, y, z) + 0.5f);
				var distanceSqr = dot(difference, difference);
				if (distanceSqr >= shortestDistanceSqr) { continue; }
				var state = volume[int3(x, y, z)];
				if (state != 0) {
					var box = Block.Find(state).Box(volume, int3(x, y, z));
					if (Intersects(ray, box, out var point, maxDistance)) {
						shortestDistanceSqr = distanceSqr;
						hit.id = state;
						hit.position = int3(x, y, z);
						hit.normal = int3((point - hit.position - 0.5f) * 2f); // TODO
					}
				}
			}
			return hit.id != 0;
		}

		static bool Intersects(Box α, Box β) {
			var Δ = α.center - β.center;
			var ε = abs(Δ) - (α.halfsize + β.halfsize);
			return all(ε <= 0);
		}

		///<summary>Tavian Barnes' method
		///at https://tavianator.com/fast-branchless-raybounding-box-intersections-part-2-nans/</summary>
		static bool Intersects(Ray ray, Box box, out float3 point, float maxDistance) {
			var t1 = (box.min[0] - ray.origin[0]) * ray.inverseDirection[0];
			var t2 = (box.max[0] - ray.origin[0]) * ray.inverseDirection[0];
			var tmin = min(t1, t2);
			var tmax = max(t1, t2);
			for (int i = 1; i < 3; ++i) {
				t1 = (box.min[i] - ray.origin[i]) * ray.inverseDirection[i];
				t2 = (box.max[i] - ray.origin[i]) * ray.inverseDirection[i];
				tmin = max(tmin, min(min(t1, t2), tmax));
				tmax = min(tmax, max(max(t1, t2), tmin));
			}
			point = ray.origin + ray.direction * tmin;
			return tmax > max(tmin, 0f) && tmin < maxDistance;
		}
	}
}
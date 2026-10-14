using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace VaMMCP.Api {
	/// <summary>
	/// Sizes and surfaces: what an atom occupies (atom_bounds) and what a ray from a point hits (probe), read from
	/// Unity's renderers and colliders instead of guessed from pictures.
	/// </summary>
	public partial class VaMApi {
		// ===================== measure =====================

		/// <summary>
		/// The space an atom takes: a box around its visible meshes and one around its colliders (what physics stands
		/// on), both world-aligned, plus the visible size along the atom's own axes. A person also reports its top and
		/// soles. Only the atom's own parts count, not atoms parented to it.
		/// </summary>
		public JSONNode AtomBounds(JSONNode args) {
			Atom a = FindAtom(S(args, "uid"));
			Transform frame = Placement(a);
			bool hasVisual = false, hasPhysical = false, hasLocal = false;
			Bounds visual = new Bounds(), physical = new Bounds(), local = new Bounds();
			int renderers = 0, colliders = 0, handleParts = 0;
			Dictionary<string, int> layers = new Dictionary<string, int>();

			foreach (Renderer rend in a.GetComponentsInChildren<Renderer>()) {
				if (!rend.enabled || !(rend is MeshRenderer || rend is SkinnedMeshRenderer)) continue;
				if (rend.GetComponentInParent<Atom>() != a) continue;
				if (IsControlHandle(rend.transform)) { handleParts++; continue; }
				renderers++;
				Count(layers, LayerMask.LayerToName(rend.gameObject.layer));
				Grow(ref visual, ref hasVisual, rend.bounds);
				foreach (Vector3 corner in Corners(rend)) {
					Vector3 p = frame.InverseTransformPoint(corner);
					if (!hasLocal) { local = new Bounds(p, Vector3.zero); hasLocal = true; } else local.Encapsulate(p);
				}
			}
			foreach (Collider col in a.GetComponentsInChildren<Collider>()) {
				if (!col.enabled || col.isTrigger) continue;
				if (col.GetComponentInParent<Atom>() != a) continue;
				if (IsControlHandle(col.transform)) { handleParts++; continue; }
				colliders++;
				Grow(ref physical, ref hasPhysical, col.bounds);
			}

			JSONClass r = new JSONClass();
			r["uid"] = a.uid;
			r["type"] = a.type;
			if (hasVisual) r["visual"] = BoundsJson(visual);
			if (hasPhysical) r["physical"] = BoundsJson(physical);
			if (hasLocal) {
				r["local_size"] = Vec(Vector3.Scale(local.size, frame.lossyScale));
				r["local_center"] = Vec(frame.TransformPoint(local.center));
			}
			if (a.type == "Person") AddPersonExtents(a, r);
			r["renderers"] = JSONRaw.Num(renderers);
			r["colliders"] = JSONRaw.Num(colliders);
			r["skipped_handle_parts"] = JSONRaw.Num(handleParts);
			JSONClass layerCounts = new JSONClass();
			foreach (KeyValuePair<string, int> kv in layers) layerCounts[kv.Key] = JSONRaw.Num(kv.Value);
			r["renderer_layers"] = layerCounts;
			if (!hasVisual && !hasPhysical) r["note"] = "no enabled meshes or colliders (an invisible atom such as a light, or switched off)";
			return r;
		}

		/// <summary>
		/// Casts a ray and reports what it hits: point, distance, surface normal, the atom and collider. Default straight
		/// down, which finds the floor, a seat or a bed top under a point. Skips triggers and VaM's control handles;
		/// ignore lists atoms to see through (a person standing where you measure).
		/// </summary>
		public JSONNode Probe(JSONNode args) {
			if (!Has(args, "from")) throw new ApiError("from required: [x,y,z] or \"Atom[/control]\"");
			JSONNode fromArg = args["from"];
			Vector3 from = fromArg.AsArray != null ? ParseVec3(fromArg) : ResolveSnapshotTarget(fromArg.Value).Point;
			Vector3 dir = Has(args, "direction") ? ParseVec3(args["direction"]) : Vector3.down;
			if (dir.sqrMagnitude < 1e-6f) throw new ApiError("direction must not be zero");
			dir.Normalize();
			float maxDistance = Num(args, "max_distance", 20f);
			int limit = Bool(args, "all", false) ? 10 : 1;
			HashSet<Atom> ignore = new HashSet<Atom>();
			if (Has(args, "ignore")) {
				JSONArray list = args["ignore"].AsArray;
				if (list == null) throw new ApiError("ignore is a list of atom uids");
				for (int i = 0; i < list.Count; i++) ignore.Add(FindAtom(list[i].Value));
			}

			RaycastHit[] hits = Physics.RaycastAll(from, dir, maxDistance, ~0, QueryTriggerInteraction.Ignore);
			System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
			JSONArray found = new JSONArray();
			foreach (RaycastHit hit in hits) {
				if (found.Count >= limit) break;
				if (IsControlHandle(hit.collider.transform)) continue;
				Atom owner = hit.collider.GetComponentInParent<Atom>();
				if (owner != null && ignore.Contains(owner)) continue;
				JSONClass h = new JSONClass();
				h["point"] = Vec(hit.point);
				h["distance"] = F(hit.distance);
				h["normal"] = Vec(hit.normal);
				h["atom"] = owner != null ? owner.uid : "";
				h["collider"] = hit.collider.name;
				h["layer"] = LayerMask.LayerToName(hit.collider.gameObject.layer);
				found.Add(h);
			}

			JSONClass r = new JSONClass();
			r["from"] = Vec(from);
			r["direction"] = Vec(dir);
			r["hits"] = found;
			if (found.Count == 0) r["note"] = "nothing hit within " + maxDistance.ToString("0.##") + " m";
			return r;
		}

		/// <summary>
		/// A control's own gizmo (the handle VaM draws to grab it) is not part of what the atom occupies: anything under a
		/// FreeControllerV3 that is not under the body part it drives (followWhenOff).
		/// </summary>
		private static bool IsControlHandle(Transform t) {
			FreeControllerV3 fc = t.GetComponentInParent<FreeControllerV3>();
			if (fc == null) return false;
			return fc.followWhenOff == null || !t.IsChildOf(fc.followWhenOff);
		}

		/// <summary>A person's top (the highest collider) and soles (the lowest), from its body colliders.</summary>
		private static void AddPersonExtents(Atom a, JSONClass r) {
			float top = float.MinValue, bottom = float.MaxValue;
			foreach (Collider col in a.GetComponentsInChildren<Collider>()) {
				if (!col.enabled || col.isTrigger || col.GetComponentInParent<Atom>() != a || IsControlHandle(col.transform)) continue;
				top = Mathf.Max(top, col.bounds.max.y);
				bottom = Mathf.Min(bottom, col.bounds.min.y);
			}
			if (top == float.MinValue) return;
			r["top"] = F(top);
			r["soles"] = F(bottom);
			r["height"] = F(top - bottom);
		}

		private static IEnumerable<Vector3> Corners(Renderer rend) {
			MeshFilter mf = rend.GetComponent<MeshFilter>();
			if (rend is MeshRenderer && mf != null && mf.sharedMesh != null) {
				Bounds b = mf.sharedMesh.bounds;
				foreach (Vector3 c in BoxCorners(b)) yield return rend.transform.TransformPoint(c);
			} else {
				foreach (Vector3 c in BoxCorners(rend.bounds)) yield return c;
			}
		}

		private static IEnumerable<Vector3> BoxCorners(Bounds b) {
			for (int i = 0; i < 8; i++) {
				yield return new Vector3(
					(i & 1) == 0 ? b.min.x : b.max.x,
					(i & 2) == 0 ? b.min.y : b.max.y,
					(i & 4) == 0 ? b.min.z : b.max.z);
			}
		}

		private static void Grow(ref Bounds total, ref bool has, Bounds b) {
			if (!has) { total = b; has = true; } else total.Encapsulate(b);
		}

		private static void Count(Dictionary<string, int> counts, string key) {
			if (string.IsNullOrEmpty(key)) key = "(unnamed)";
			int n;
			counts.TryGetValue(key, out n);
			counts[key] = n + 1;
		}

		private static JSONClass BoundsJson(Bounds b) {
			JSONClass j = new JSONClass();
			j["min"] = Vec(b.min);
			j["max"] = Vec(b.max);
			j["size"] = Vec(b.size);
			j["center"] = Vec(b.center);
			j["top"] = F(b.max.y);
			j["bottom"] = F(b.min.y);
			return j;
		}
	}
}

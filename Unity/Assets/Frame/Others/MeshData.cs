using System;
using System.Collections.Generic;
using System.Linq;
using QuadTreeLib;
using UnityEngine;
using VectorStructExtensions;
using Object = UnityEngine.Object;

public class VRaycastHit {
	public VRaycastHit() {
		distance = 0;
		//textureCoord = Vector2.zero;
		barycentricCoordinate = VVector2.zero;
		point = VVector3.zero;
	}
	public VRaycastHit(double distance, VVector2 barycentricCoordinate) {
		this.distance = distance;
		this.barycentricCoordinate = barycentricCoordinate;
		//textureCoord = Vector2.zero;
		point = VVector3.zero;
	}

	public double distance;
	public VVector2 barycentricCoordinate;
	//public VVector2 textureCoord;
	public VVector3 point;
}

public class MeshData
{
	public List<VVector3> vertexes = new List<VVector3>();
	public List<VVector3> normals = new List<VVector3>();
	public List<VVector4> tangents = new List<VVector4>();
	public List<Color> colors = new List<Color>();
	public List<VVector2> uvs = new List<VVector2>();
	public List<VVector2> uv2s = new List<VVector2>();
	public List<VVector2> uv3s = new List<VVector2>();
	public List<VVector2> uv4s = new List<VVector2>();
	public List<BoneWeight> boneWeights = new List<BoneWeight>();
	// maybe todo: add support for bind-poses
	public List<List<int>> submeshTriangleVertexIndexes = new List<List<int>>();

	// allow to precompute list.ToArray in threads
	public Vector3[] vertexesArray;
	public Vector3[] normalsArray;
	public Vector4[] tangentsArray;
	public Color[] colorsArray;
	public Vector2[] uvsArray;
	public Vector2[] uv2sArray;
	public Vector2[] uv3sArray;
	public Vector2[] uv4sArray;
	public BoneWeight[] boneWeightsArray;
	public List<int[]> submeshTriangleVertexIndexesArrays;

	// maybe temp; special, for use by terrain chunk builder
	/*public List<VVector3> normals_base = new List<VVector3>();
	public List<VVector4> tangents_base = new List<VVector4>();
	public readonly Dictionary<VVector3, int> vertexCache = new Dictionary<VVector3, int>();*/

	public void Clear() {
		vertexes.Clear();
		normals.Clear();
		tangents.Clear();
		colors.Clear();
		uvs.Clear();
		uv2s.Clear();
		uv3s.Clear();
		uv4s.Clear();
		boneWeights.Clear();
		submeshTriangleVertexIndexes.Clear();

		// maybe temp
		/*normals_base.Clear();
		tangents_base.Clear();
		vertexCache.Clear();*/

		vertexesArray = null;
		normalsArray = null;
		tangentsArray = null;
		colorsArray = null;
		uvsArray = null;
		uv2sArray = null;
		uv3sArray = null;
		uv4sArray = null;
		boneWeightsArray = null;
		submeshTriangleVertexIndexesArrays = null;
	}

	public void PrepareArrays() {
		vertexesArray = vertexes.Select(a=>a.ToVector3()).ToArray();
		normalsArray = normals.Select(a=>a.ToVector3()).ToArray();
		tangentsArray = tangents.Select(a=>a.ToVector4()).ToArray();
		colorsArray = colors.ToArray();
		uvsArray = uvs.Select(a=>a.ToVector2(false)).ToArray();
		uv2sArray = uv2s.Select(a=>a.ToVector2(false)).ToArray();
		uv3sArray = uv3s.Select(a=>a.ToVector2(false)).ToArray();
		uv4sArray = uv4s.Select(a=>a.ToVector2(false)).ToArray();
		boneWeightsArray = boneWeights.ToArray();
		submeshTriangleVertexIndexesArrays = submeshTriangleVertexIndexes.Select(a=>a.ToArray()).ToList();
	}

	public static MeshData FromMesh(Mesh mesh) {
		var result = new MeshData();
		result.vertexes = mesh.vertices.Select(a=>a.ToVVector3()).ToList();
		result.normals = mesh.normals.Select(a=>a.ToVVector3()).ToList();
		result.tangents = mesh.tangents.Select(a=>a.ToVVector4()).ToList();
		result.colors = mesh.colors.ToList();
		result.uvs = mesh.uv.Select(a=>a.ToVVector2(false)).ToList();
		result.uv2s = mesh.uv2.Select(a=>a.ToVVector2(false)).ToList();
		result.uv3s = mesh.uv3.Select(a=>a.ToVVector2(false)).ToList();
		result.uv4s = mesh.uv4.Select(a=>a.ToVVector2(false)).ToList();
		result.boneWeights = mesh.boneWeights.ToList();
		result.submeshTriangleVertexIndexes = new List<List<int>>();
		for (var i = 0; i < mesh.subMeshCount; i++)
			result.submeshTriangleVertexIndexes.Add(mesh.GetTriangles(i).ToList());
		return result;
	}
	public Mesh ToMesh(Mesh mesh) {
		if (vertexes.Count == 0) {
			if (mesh != null)
				if (!Application.isEditor)
					Object.Destroy(mesh);
				else
					Object.DestroyImmediate(mesh);
			return null;
		}

		if (mesh == null)
			mesh = new Mesh();

		if (vertexesArray == null) {
			Debug.LogError("Please call meshData.PrepareArrays() before meshData.ToMesh()");
			return null;
		}

		mesh.Clear();
		mesh.vertices = vertexesArray;
		mesh.normals = normalsArray;
		mesh.tangents = tangentsArray;
		mesh.colors = colorsArray;
		mesh.uv = uvsArray;
		mesh.uv2 = uv2sArray;
		mesh.uv3 = uv3sArray;
		mesh.uv4 = uv4sArray;
		mesh.boneWeights = boneWeightsArray;
		mesh.subMeshCount = submeshTriangleVertexIndexesArrays.Count;
		for (var i = 0; i < submeshTriangleVertexIndexesArrays.Count; i++)
			mesh.SetTriangles(submeshTriangleVertexIndexesArrays[i], i);

		return mesh;
	}

	public class Triangle : IHasRect {
		public Triangle(VVector3 point1, VVector3 point2, VVector3 point3) {
			this.point1 = point1;
			this.point2 = point2;
			this.point3 = point3;

			var posX = Math.Min(Math.Min(point1.x, point2.x), point3.x); //V.Min(point1.x, point2.x, point3.x));
			var posY = Math.Min(Math.Min(point1.y, point2.y), point3.y); //V.Min(point1.z, point2.z, point3.z);
			//rect = new VRect(posX, posY, 1 + (V.Max(point1.x, point2.x, point3.x) - posX), 1 + (V.Max(point1.z, point2.z, point3.z) - posY));
			rect = new VRect(posX, posY, Math.Max(Math.Max(point1.x, point2.x), point3.x) - posX, Math.Max(Math.Max(point1.y, point2.y), point3.y) - posY);
		}

		public VVector3 point1;
		public VVector3 point2;
		public VVector3 point3;

		public VRect rect;

		public VRect GetBoundsRect() { return rect; }
	}

	// cache - raycasting system (when raycasting straight down)
	QuadTreeNode<Triangle> cache_triangles_tree;
	public void ClearCache() { cache_triangles_tree = null; }

	// raycasting system
	public VRaycastHit GetRayHit(VRay ray, float maxDist = float.MaxValue) { return GetRayHits(ray, maxDist).FirstOrDefault(); }
	public List<VRaycastHit> GetRayHits(VRay ray, double maxDist = float.MaxValue) {
		var triangleVertexIndexes = submeshTriangleVertexIndexes[0]; // temp; only process the first submesh

		var hits = new List<VRaycastHit>();
		if (ray.direction.x == 0 && ray.direction.y == 0) {
			if (cache_triangles_tree == null) {
				var triangles = new List<Triangle>();
				var bounds = VRect.Null;
				for (int i = 0; i < triangleVertexIndexes.Count; i += 3) {
					var triangle = new Triangle(vertexes[triangleVertexIndexes[i]], vertexes[triangleVertexIndexes[i + 1]], vertexes[triangleVertexIndexes[i + 2]]);
					bounds.Encapsulate(triangle.rect);
					triangles.Add(triangle);
				}
				bounds = bounds.Grow(.1); // fix for rounding errors
				var triangles_tree = new QuadTreeNode<Triangle>(bounds);
				foreach (Triangle triangle in triangles)
					triangles_tree.Insert(triangle);
				cache_triangles_tree = triangles_tree;
			}

			foreach (Triangle triangle in cache_triangles_tree.GetItemsIntersecting(new VRect(ray.origin.ToVVector2(), VVector2.one))) {
				double distance;
				VVector2 barycentricCoordinate;
				if (TestIntersection(triangle.point1, triangle.point2, triangle.point3, ray, out distance, out barycentricCoordinate) && distance <= maxDist) {
					var newHit = new VRaycastHit(distance, barycentricCoordinate);
					//newHit.textureCoord = uvs[point1Index] + ((uvs[point2Index] - uvs[point1Index]) * barycentricCoordinate.x) + ((uvs[point3Index] - uvs[point1Index]) * barycentricCoordinate.y);
					//newHit.point = (vertices[point1Index] + vertices[point2Index] + vertices[point3Index]) / 3; // use the triangle's center as the 'hit point' (a close approximate, though not accurate)
					newHit.point = ray.origin + (ray.direction * distance);
					hits.Add(newHit);
				}
			}
		}
		else
			for (int i = 0; i < triangleVertexIndexes.Count; i += 3) {
				var point1Index = triangleVertexIndexes[i];
				var point2Index = triangleVertexIndexes[i + 1];
				var point3Index = triangleVertexIndexes[i + 2];
				double distance;
				VVector2 barycentricCoordinate;
				if (TestIntersection(vertexes[point1Index], vertexes[point2Index], vertexes[point3Index], ray, out distance, out barycentricCoordinate) && distance <= maxDist) {
					var newHit = new VRaycastHit(distance, barycentricCoordinate);
					//newHit.textureCoord = uvs[point1Index] + ((uvs[point2Index] - uvs[point1Index]) * barycentricCoordinate.x) + ((uvs[point3Index] - uvs[point1Index]) * barycentricCoordinate.y);
					//newHit.point = (vertices[point1Index] + vertices[point2Index] + vertices[point3Index]) / 3; // use the triangle's center as the 'hit point' (a close approximate, though not accurate)
					newHit.point = ray.origin + (ray.direction * distance);
					hits.Add(newHit);
				}
			}

		// sort results by distance
		bool pointsWereSwapped = true;
		while (pointsWereSwapped) {
			pointsWereSwapped = false;
			for (int i = 1; i < hits.Count; i++)
				if (hits[i - 1].distance > hits[i].distance) {
					VRaycastHit a = hits[i - 1];
					VRaycastHit b = hits[i];
					hits[i - 1] = b;
					hits[i] = a;
					pointsWereSwapped = true;
				}
		}

		return hits;
	}

	/// <summary>Tests the intersection. Implementation of the Moller/Trumbore intersection algorithm.</summary>
	/// <returns>True if the ray does intersect; out distance: the distance along the ray at the intersection point; out hitPoint</returns>
	/// <param name="distance">Distance of hit surface from ray origin.</param>
	/// <param name="baryCoord">Barycentric coordinate of the intersection point.</param>
	/// http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf
	bool TestIntersection(VVector3 point1, VVector3 point2, VVector3 point3, VRay ray, out double distance, out VVector2 baryCoord)
	{
		baryCoord = VVector2.zero;
		distance = Mathf.Infinity;
		VVector3 edge1 = point2 - point1;
		VVector3 edge2 = point3 - point1;
		VVector3 pVec = VVector3.Cross(ray.direction, edge2);
		double det = VVector3.Dot(edge1, pVec);
		if (det < .0001) // epsilon
			return false;

		VVector3 tVec = ray.origin - point1;
		double u = VVector3.Dot(tVec, pVec);
		if (u < 0 || u > det)
			return false;

		VVector3 qVec = VVector3.Cross(tVec, edge1);
		double v = VVector3.Dot(ray.direction, qVec);
		if (v < 0 || u + v > det)
			return false;

		distance = VVector3.Dot(edge2, qVec);
		double invDet = 1 / det;
		distance *= invDet;
		baryCoord.x = u * invDet;
		baryCoord.y = v * invDet;
		return true;
	}

	// mesh optimization
	// ==========

	public class VertexMergeInfo
	{
		public int to_vertexIndexInOldMesh;
		public int to_vertexIndexInNewMesh;
		//public List<int> fromVertexIndexes = new List<int>();
		public VertexMergeInfo(int to_vertexIndexInOldMesh, int to_vertexIndexInNewMesh)
		{
			this.to_vertexIndexInOldMesh = to_vertexIndexInOldMesh;
			this.to_vertexIndexInNewMesh = to_vertexIndexInNewMesh;
		}
	}

	/*public static void OptimizeMesh(Mesh mesh)
	{
		var vertexes = mesh.vertices.ToList();
		var normals = mesh.normals.ToList();
		var uvs = mesh.uv.ToList();
		var submeshTriangles = new List<List<int>>();
		for (var i = 0; i < mesh.subMeshCount; i++)
			submeshTriangles.Add(mesh.GetTriangles(i).ToList());
		var boneWeights = mesh.boneWeights.ToList();
		OptimizeMesh(vertexes, normals, uvs, submeshTriangles, boneWeights);
		mesh.vertices = vertexes.ToArray();
		mesh.normals = normals.ToArray();
		mesh.uv = uvs.ToArray();
		for (var i = 0; i < mesh.subMeshCount; i++)
			mesh.SetTriangles(submeshTriangles[i].ToArray(), i);
		mesh.boneWeights = boneWeights.ToArray();
	}*/
	public void Optimize()
	{
		/*normals = normals ?? Enumerable.Range(0, vertexes.Count).Select(a => new Vector3()).ToList();
		uvs = uvs ?? Enumerable.Range(0, vertexes.Count).Select(a => new Vector2()).ToList();
		boneWeights = boneWeights ?? Enumerable.Range(0, vertexes.Count).Select(a => new BoneWeight()).ToList();*/

		// calculate merges
		var vertexInfoStrings = new List<string>();
		var vertexMergeInfo_byVertexInfoStr = new Dictionary<string, VertexMergeInfo>();
		var mergedVertexes = 0;
		for (var i = 0; i < vertexes.Count; i++)
		{
			var vertex = vertexes[i];
			var vertexInfoStr = "{position:[" + vertex + "]";
			if (normals.Count > 0)
				vertexInfoStr += " normal:[" + normals[i] + "]";
			if (tangents.Count > 0)
				vertexInfoStr += " tangent:[" + tangents[i] + "]";
			if (colors.Count > 0)
				vertexInfoStr += " color:[" + colors[i] + "]";
			if (uvs.Count > 0)
				vertexInfoStr += " uv:[" + uvs[i] + "]";
			if (boneWeights.Count > 0)
			{
				var bW = boneWeights[i];
				string boneWeightStr = bW.boneIndex0 + " " + bW.weight0 + " " + bW.boneIndex1 + " " + bW.weight1 + " " + bW.boneIndex2 + " " + bW.weight2 + " " + bW.boneIndex3 + " " + bW.weight3;
				vertexInfoStr += " boneWeight:[" + boneWeightStr + "]";
			}
			vertexInfoStr += "}";

			vertexInfoStrings.Add(vertexInfoStr);
			if (!vertexMergeInfo_byVertexInfoStr.ContainsKey(vertexInfoStr))
			{
				var newIndex = i - mergedVertexes;
				vertexMergeInfo_byVertexInfoStr.Add(vertexInfoStr, new VertexMergeInfo(i, newIndex));
			}
			else
			{
				//vertexMergeInfo_byVertexInfoStr[vertexInfoStr].fromVertexIndexes.Add(i);
				mergedVertexes++;
			}
		}

		// apply merges
		var newVertexes = new List<VVector3>();
		var newNormals = new List<VVector3>();
		var newTangents = new List<VVector4>();
		var newColors = new List<Color>();
		var newUVs = new List<VVector2>();
		var newBoneWeights = new List<BoneWeight>();
		for (var i = 0; i < vertexes.Count; i++)
		{
			var vertexInfoStr = vertexInfoStrings[i];
			var vertexMergeInfo = vertexMergeInfo_byVertexInfoStr[vertexInfoStr];
			if (vertexMergeInfo.to_vertexIndexInOldMesh == i) // if not being removed/merged-with-an-earlier-vertex
			{
				newVertexes.Add(vertexes[i]);
				if (normals.Count > 0)
					newNormals.Add(normals[i]);
				if (tangents.Count > 0)
					newTangents.Add(tangents[i]);
				if (colors.Count > 0)
					newColors.Add(colors[i]);
				if (uvs.Count > 0)
					newUVs.Add(uvs[i]);
				if (boneWeights.Count > 0)
					newBoneWeights.Add(boneWeights[i]);
			}
		}
		var newSubmeshTriangleVertexIndexes = new List<List<int>>();
		for (var submeshIndex = 0; submeshIndex < submeshTriangleVertexIndexes.Count; submeshIndex++)
		{
			var currentSubmeshTriangleVertexIndexes = submeshTriangleVertexIndexes[submeshIndex];
			var newCurrentSubmeshTriangleVertexIndexes = new List<int>();
			for (var i = 0; i < currentSubmeshTriangleVertexIndexes.Count; i++)
			{
				var triangleVertexIndex = currentSubmeshTriangleVertexIndexes[i];
				//var triangleVertex = vertexes[triangleVertexIndex];
				var triangleVertexInfoStr = vertexInfoStrings[triangleVertexIndex];
				var triangleVertexMergeInfo = vertexMergeInfo_byVertexInfoStr[triangleVertexInfoStr];

				//if (triangleVertexMergeInfo.toVertexIndex == triangleVertexIndex) // if not being removed/merged-with-an-earlier-vertex
				//	newCurrentSubmeshTriangleVertexIndexes.Add(triangleVertexIndex);

				var newTriangleVertexIndex = triangleVertexMergeInfo.to_vertexIndexInNewMesh;
				newCurrentSubmeshTriangleVertexIndexes.Add(newTriangleVertexIndex);
			}
			newSubmeshTriangleVertexIndexes.Add(newCurrentSubmeshTriangleVertexIndexes);
		}
		vertexes = newVertexes;
		normals = newNormals;
		tangents = newTangents;
		colors = newColors;
		uvs = newUVs;
		boneWeights = newBoneWeights;
		submeshTriangleVertexIndexes = newSubmeshTriangleVertexIndexes;
	}

	public Bounds GetChildrenBounds()
	{
		var minPos = VVector3.one * 5000000;
		var maxPos = VVector3.one * -5000000;
		foreach (VVector3 vertex in vertexes)
		{
			minPos = VVector3.Min(minPos, vertex);
			maxPos = VVector3.Max(maxPos, vertex);
		}
		var size = maxPos - minPos;
		return new VBounds(minPos + (size * .5), size);
	}
}
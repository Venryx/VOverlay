using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using System.Collections;
using VectorStructExtensions;

public static class BuildHelper {
	public static void CalculateUVs(MeshData mesh) {
		foreach (VVector3 ver in mesh.vertexes) // loop through vertices, and update their uv's
			mesh.uvs.Add(new VVector2(ver.x, ver.z));
	}

	public static void CalculateNormals(MeshData mesh) {
		var normals = new VVector3[mesh.vertexes.Count];

		for (var i = 0; i < normals.Length; i++)
			normals[i] = VVector3.zero;

		for (var i = 0; i < mesh.submeshTriangleVertexIndexes[0].Count; i += 3) // maybe todo: add support for additional submeshes
		{
			int i1 = mesh.submeshTriangleVertexIndexes[0][i + 0];
			int i2 = mesh.submeshTriangleVertexIndexes[0][i + 1];
			int i3 = mesh.submeshTriangleVertexIndexes[0][i + 2];

			// get the three vertices that make the faces
			VVector3 p1 = mesh.vertexes[i1];
			VVector3 p2 = mesh.vertexes[i2];
			VVector3 p3 = mesh.vertexes[i3];

			VVector3 v1 = p2 - p1;
			VVector3 v2 = p3 - p1;
			VVector3 normal = VVector3.Cross(v1, v2);
			//normal.Normalize();

			// todo (maybe old); fix normals issue

			normals[i1] += normal;
			normals[i2] += normal;
			normals[i3] += normal;
		}

		/*mesh.normals_base.Clear();
		mesh.normals_base.Capacity = normals.Length; //+ 16;*/
		mesh.normals.Clear();
		mesh.normals.Capacity = normals.Length; //+ 16;
		for (int i = 0; i < normals.Length; i++)
		{
			VVector3 normal = normals[i];
			normal = normal.normalized;
			//mesh.normals_base.Add(normal);
			mesh.normals.Add(normal);
		}
	}

	public static void CalculateTangents(MeshData mesh)
	{
		int vertexCount = mesh.vertexes.Count;

		var tan1 = new VVector3[vertexCount];
		var tan2 = new VVector3[vertexCount];

		for (int a = 0; a < mesh.submeshTriangleVertexIndexes[0].Count; a += 3)
		{
			int i1 = mesh.submeshTriangleVertexIndexes[0][a + 0];
			int i2 = mesh.submeshTriangleVertexIndexes[0][a + 1];
			int i3 = mesh.submeshTriangleVertexIndexes[0][a + 2];

			VVector3 v1 = mesh.vertexes[i1];
			VVector3 v2 = mesh.vertexes[i2];
			VVector3 v3 = mesh.vertexes[i3];

			VVector2 w1 = mesh.uvs[i1];
			VVector2 w2 = mesh.uvs[i2];
			VVector2 w3 = mesh.uvs[i3];

			double x1 = v2.x - v1.x;
			double x2 = v3.x - v1.x;
			double y1 = v2.y - v1.y;
			double y2 = v3.y - v1.y;
			double z1 = v2.z - v1.z;
			double z2 = v3.z - v1.z;

			double s1 = w2.x - w1.x;
			double s2 = w3.x - w1.x;
			double t1 = w2.y - w1.y;
			double t2 = w3.y - w1.y;

			double div = s1 * t2 - s2 * t1;
			double r = Mathf.Approximately((float)div, 0) ? 0 : 1 / div;

			var sdir = new VVector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			var tdir = new VVector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

			tan1[i1] += sdir;
			tan1[i2] += sdir;
			tan1[i3] += sdir;

			tan2[i1] += tdir;
			tan2[i2] += tdir;
			tan2[i3] += tdir;
		}

		//mesh.tangents_base.Clear();
		mesh.tangents.Clear();
		for (var a = 0; a < vertexCount; ++a)
		{
			VVector3 n = mesh.normals[a];
			VVector3 t = tan1[a];

			VVector3 tmp = (t - n * VVector3.Dot(n, t)).normalized;
			double w = (VVector3.Dot(VVector3.Cross(n, t), tan2[a]) < 0d) ? -1 : 1;
			//mesh.tangents_base.Add(new VVector4(tmp.x, tmp.y, tmp.z, w));
			mesh.tangents.Add(new VVector4(tmp.x, tmp.y, tmp.z, w));
		}
	}

	/// <summary>Harmonize all twin vertices of the mesh.</summary> // note; may need cleanup // todo; double-check this
	/*public static int HarmonizeTwinVertices(Chunk chunk, MeshData mesh)
	{
		return 0; // temp

		var result = 0; // neighborsNotYetHarmonizedWith

		var normalValuesBySelfVertexIndexes = new Dictionary<int, List<Vector3>>();
		var tangentValuesBySelfVertexIndexes = new Dictionary<int, List<Vector4>>();
			
		// iterate over neighbors
		foreach (Vector3i dir in Vector3i.allDirections)
		{
			Chunk neighbor = chunk.GetNeighbor(dir);
			if (neighbor == null)
				continue;
			MeshData neighborMesh = neighbor.meshData_tempForTwinVertexHarmonization;
			if (neighborMesh == null || neighborMesh.vertices.Count == 0/* || !neighbor.hasBeenPostBuilt*#/) // todo: double-check this
				continue;

			Debug.Log("Harmonizing-1 " + chunk.position + " with " + neighbor.position);

			int startX = dir.x == 1 ? Chunk.CHUNK_BLOCKSPERAXIS : 0;
			int endX = dir.x == -1 ? 0 : Chunk.CHUNK_BLOCKSPERAXIS;
			int borderX_forNeighbor = dir.x == -1 ? Chunk.CHUNK_BLOCKSPERAXIS : (dir.x == 1 ? -Chunk.CHUNK_BLOCKSPERAXIS : 0);

			int startY = dir.y == 1 ? Chunk.CHUNK_BLOCKSPERAXIS : 0;
			int endY = dir.y == -1 ? 0 : Chunk.CHUNK_BLOCKSPERAXIS;
			int borderY_forNeighbor = dir.y == -1 ? Chunk.CHUNK_BLOCKSPERAXIS : (dir.y == 1 ? -Chunk.CHUNK_BLOCKSPERAXIS : 0);

			int startZ = dir.z == 1 ? Chunk.CHUNK_BLOCKSPERAXIS : 0;
			int endZ = dir.z == -1 ? 0 : Chunk.CHUNK_BLOCKSPERAXIS;
			int borderZ_forNeighbor = dir.z == -1 ? Chunk.CHUNK_BLOCKSPERAXIS : (dir.z == 1 ? -Chunk.CHUNK_BLOCKSPERAXIS : 0);

			int vertexIndex, vertexIndex_forNeighbor;
			for (int x = startX; x <= endX; x++)
				for (int y = startY; y <= endY; y++)
					for (int z = startZ; z <= endZ; z++)
					{
						int cacheIndex = x + MeshData.CACHE_SIZE_X * (y + MeshData.CACHE_SIZE_Y * z);
						int nCacheIndex = (x + borderX_forNeighbor) + MeshData.CACHE_SIZE_X * ((y + borderY_forNeighbor) + MeshData.CACHE_SIZE_Y * (z + borderZ_forNeighbor));

						// CACHE X
						if (mesh.edgeVertexCacheX.TryGetValue(cacheIndex, out vertexIndex))
							if (neighborMesh.edgeVertexCacheX.TryGetValue(nCacheIndex, out vertexIndex_forNeighbor))
							{
								if (!normalValuesBySelfVertexIndexes.ContainsKey(vertexIndex))
								{
									normalValuesBySelfVertexIndexes.Add(vertexIndex, new List<Vector3> {mesh.normals_base[vertexIndex]});
									tangentValuesBySelfVertexIndexes.Add(vertexIndex, new List<Vector4> {mesh.tangents_base[vertexIndex]});
								}
								normalValuesBySelfVertexIndexes[vertexIndex].Add(neighborMesh.normals_base[vertexIndex_forNeighbor]);
								tangentValuesBySelfVertexIndexes[vertexIndex].Add(neighborMesh.tangents_base[vertexIndex_forNeighbor]);
							}
						// CACHE Y
						if (mesh.edgeVertexCacheY.TryGetValue(cacheIndex, out vertexIndex))
							if (neighborMesh.edgeVertexCacheY.TryGetValue(nCacheIndex, out vertexIndex_forNeighbor))
							{
								if (!normalValuesBySelfVertexIndexes.ContainsKey(vertexIndex))
								{
									normalValuesBySelfVertexIndexes.Add(vertexIndex, new List<Vector3> {mesh.normals_base[vertexIndex]});
									tangentValuesBySelfVertexIndexes.Add(vertexIndex, new List<Vector4> {mesh.tangents_base[vertexIndex]});
								}
								normalValuesBySelfVertexIndexes[vertexIndex].Add(neighborMesh.normals_base[vertexIndex_forNeighbor]);
								tangentValuesBySelfVertexIndexes[vertexIndex].Add(neighborMesh.tangents_base[vertexIndex_forNeighbor]);
							}
						// CACHE Z
						if (mesh.edgeVertexCacheZ.TryGetValue(cacheIndex, out vertexIndex))
							if (neighborMesh.edgeVertexCacheZ.TryGetValue(nCacheIndex, out vertexIndex_forNeighbor))
							{
								if (!normalValuesBySelfVertexIndexes.ContainsKey(vertexIndex))
								{
									normalValuesBySelfVertexIndexes.Add(vertexIndex, new List<Vector3> {mesh.normals_base[vertexIndex]});
									tangentValuesBySelfVertexIndexes.Add(vertexIndex, new List<Vector4> {mesh.tangents_base[vertexIndex]});
								}
								normalValuesBySelfVertexIndexes[vertexIndex].Add(neighborMesh.normals_base[vertexIndex_forNeighbor]);
								tangentValuesBySelfVertexIndexes[vertexIndex].Add(neighborMesh.tangents_base[vertexIndex_forNeighbor]);
							}
					}
		}

		// iterate over neighbors
		foreach (Vector3i dir in Vector3i.allDirections)
		{
			Chunk neighbor = chunk.GetNeighbor(dir);
			if (neighbor == null)
				continue;
			MeshData nMesh = neighbor.meshData_tempForTwinVertexHarmonization;
			if (nMesh == null || nMesh.vertices.Count == 0/* || !neighbor.hasBeenPostBuilt*#/) // todo; double-check this
			{
				if (neighbor.beingBuilt) // it's still in the building process
					result++;
				continue;
			}

			Debug.Log("Harmonizing-2 " + chunk.position + " with " + neighbor.position);

			int nx = dir.x == -1 ? Chunk.CHUNK_BLOCKSPERAXIS : (dir.x == 1 ? -Chunk.CHUNK_BLOCKSPERAXIS : 0);
			int startX = dir.x == 1 ? Chunk.CHUNK_BLOCKSPERAXIS : 0;
			int endX = dir.x == -1 ? 0 : Chunk.CHUNK_BLOCKSPERAXIS;

			int ny = dir.y == -1 ? Chunk.CHUNK_BLOCKSPERAXIS : (dir.y == 1 ? -Chunk.CHUNK_BLOCKSPERAXIS : 0);
			int startY = dir.y == 1 ? Chunk.CHUNK_BLOCKSPERAXIS : 0;
			int endY = dir.y == -1 ? 0 : Chunk.CHUNK_BLOCKSPERAXIS;

			int nz = dir.z == -1 ? Chunk.CHUNK_BLOCKSPERAXIS : (dir.z == 1 ? -Chunk.CHUNK_BLOCKSPERAXIS : 0);
			int startZ = dir.z == 1 ? Chunk.CHUNK_BLOCKSPERAXIS : 0;
			int endZ = dir.z == -1 ? 0 : Chunk.CHUNK_BLOCKSPERAXIS;

			int vi, nvi;
			for (int x = startX; x <= endX; x++)
				for (int y = startY; y <= endY; y++)
					for (int z = startZ; z <= endZ; z++)
					{
						int cacheIndex = x + MeshData.CACHE_SIZE_X * (y + MeshData.CACHE_SIZE_Y * z);
						int nCacheIndex = (x + nx) + MeshData.CACHE_SIZE_X * ((y + ny) + MeshData.CACHE_SIZE_Y * (z + nz));

						// CACHE X
						if (mesh.edgeVertexCacheX.TryGetValue(cacheIndex, out vi))
							if (nMesh.edgeVertexCacheX.TryGetValue(nCacheIndex, out nvi) && normalValuesBySelfVertexIndexes.ContainsKey(vi)) // this may be a new vertex that was just built on a background thread
								HarmonizeTwin(vi, nvi, mesh, nMesh, normalValuesBySelfVertexIndexes[vi], tangentValuesBySelfVertexIndexes[vi]);
						// CACHE Y
						if (mesh.edgeVertexCacheY.TryGetValue(cacheIndex, out vi))
							if (nMesh.edgeVertexCacheY.TryGetValue(nCacheIndex, out nvi) && normalValuesBySelfVertexIndexes.ContainsKey(vi))
								HarmonizeTwin(vi, nvi, mesh, nMesh, normalValuesBySelfVertexIndexes[vi], tangentValuesBySelfVertexIndexes[vi]);
						// CACHE Z
						if (mesh.edgeVertexCacheZ.TryGetValue(cacheIndex, out vi))
							if (nMesh.edgeVertexCacheZ.TryGetValue(nCacheIndex, out nvi) && normalValuesBySelfVertexIndexes.ContainsKey(vi))
								HarmonizeTwin(vi, nvi, mesh, nMesh, normalValuesBySelfVertexIndexes[vi], tangentValuesBySelfVertexIndexes[vi]);
					}

			// we've harmonized with neighbor, so diminish its to-do-list count by one
			//neighbor.neighborsNotYetHarmonizedWith--;
			nMesh.PrepareArrays(); // reconstruct its arrays (because its mesh data is about to actually be added/displayed)
		}

		return result;
	}
	static void HarmonizeTwin(int vi, int nvi, MeshData mesh, MeshData nMesh, List<Vector3> normalValues, List<Vector4> tangentValues)
	{
		// todo; get this working perfectly (you can still see issues at chunk edges, at the moment)
		try
		{
			// Harmonize normals
			//mesh.normals[vi] = nMesh.normals[nvi];
			// Harmonize colors
			//mesh.colors[vi] = nMesh.colors[nvi];
			// Harmonize tangents
			//mesh.tangents[vi] = nMesh.tangents[nvi];

			var averageNormal = V.Average_V3(normalValues);
			mesh.normals[vi] = averageNormal;
			nMesh.normals[nvi] = averageNormal;

			var averageTangent = V.Average_V4(tangentValues);
			mesh.tangents[vi] = averageTangent;
			nMesh.tangents[nvi] = averageTangent;

			//mesh.normals[vi] = (mesh.normals[vi] + nMesh.normals[nvi]) / 2;
			//mesh.tangents[vi] = (mesh.tangents[vi] + nMesh.tangents[nvi]) / 2;
		}
		catch (System.Exception e)
		{
			Debug.LogException(e);
			Debug.Log("HarmonizeTwin: vi=" + vi + " nvi=" + nvi + " mesh.vertices.Count=" + mesh.vertices.Count + " nMesh.vertices.Count=" + nMesh.vertices.Count);
			Debug.Log("HarmonizeTwin: vi=" + vi + " nvi=" + nvi + " mesh.normals.Count=" + mesh.normals.Count + " nMesh.normals.Count=" + nMesh.normals.Count);
			Debug.Log("HarmonizeTwin: vi=" + vi + " nvi=" + nvi + " mesh.colors.Count=" + mesh.colors.Count + " nMesh.colors.Count=" + nMesh.colors.Count);
		}
	}*/

	/*public static void HarmonizeVertexColors(Chunk chunk, MeshData meshData)
	{
		for (var i = 0; i < meshData.vertices.Count; i++)
		{
			var vertexInMesh = meshData.vertices[i];
			var blockOfVertexPosInBlock = new Vector3i((int)(vertexInMesh.x / Chunk.BLOCK_SIZE), (int)(vertexInMesh.y / Chunk.BLOCK_SIZE), (int)(vertexInMesh.z / Chunk.BLOCK_SIZE));
			var blockOfVertex = chunk.GetBlock_InNeighbors_1NegativeOr1Positive(blockOfVertexPosInBlock);
			if (blockOfVertex == null || blockOfVertex.chunk == chunk) // if null, or in original chunk
				continue;

			if (blockOfVertex.position.x == 0 || blockOfVertex.position.y == 0 || blockOfVertex.position.z == 0)
			{
				var ourSlotHoldingLeakInTexture = -1;
				for (int i2 = 0; i2 < chunk.newSlotTextures.Length; i2++)
					if ((object)chunk.newSlotTextures[i2] == (object)blockOfVertex.soil.texture)
						ourSlotHoldingLeakInTexture = i2;
				if (ourSlotHoldingLeakInTexture != -1 && ourSlotHoldingLeakInTexture <= 3)
					meshData.colors[i] = new List<Color> {new Color(1, 0, 0, 0), new Color(0, 1, 0, 0), new Color(0, 0, 1, 0), new Color(0, 0, 0, 1)}[ourSlotHoldingLeakInTexture];
			}
		}
	}*/
}
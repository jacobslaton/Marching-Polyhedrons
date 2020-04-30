using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
﻿using System.Linq;
using UnityEngine;

[Serializable]
public class ArrayWrapper<T>
{
	public T[] items;
	public ArrayWrapper(T[] items) { this.items = items; }
}

public class TrianglePermutation
{
	private List<List<int>> triangles;

	public TrianglePermutation(List<List<int>> triangles)
	{
		this.triangles = new List<List<int>>();
		foreach (List<int> tri in triangles)
			this.triangles.Add(new List<int>(tri));
	}
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingPolyhedron : MonoBehaviour
{
	private List<GameObject> vertexObjects;
	private List<GameObject> edgeObjects;
	private List<Vector3> geometricVertices;
	private List<Vector3> geometricEdges;
	public int[][] triangulationTable;

	private void Awake()
	{
		vertexObjects = new List<GameObject>();
		edgeObjects = new List<GameObject>();
		geometricVertices = new List<Vector3>();
		geometricEdges = new List<Vector3>();
		GeneratePolyhedron();
		//GenerateTriangulationTable();
		//WriteTriangulationTable();
	}

	private void GenerateTriangulationTable()
	{
		triangulationTable = new int[1 << vertexObjects.Count][];
		for (int tti = 0; tti < triangulationTable.Length; ++tti)
		{
			// Calculate mesh given current vertices

			vertexObjects[0].GetComponent<MarchingVertex>().SetState(true);
			for (int voi = 0; voi < vertexObjects.Count; ++voi)
				vertexObjects[voi].GetComponent<MarchingVertex>().SetState((tti & (1 << voi)) == 1 << voi);
		}
	}

	// Save and Load Triangulation Table
	private void WriteTriangulationTable()
	{
		string fstr = "";
		for (int tti = 0; tti < triangulationTable.Length; ++tti)
		{
			ArrayWrapper<int> aw = new ArrayWrapper<int>(triangulationTable[tti]);
			fstr += JsonUtility.ToJson(aw)+"\n";
		}
		fstr = fstr.Trim();

		using (StreamWriter fout = new StreamWriter("TriangulationTable.txt"))
			fout.WriteLine(fstr);
	}

	private void ReadTriangulationTable()
	{
		string file = "";
		using (StreamReader fin = new StreamReader("TriangulationTable.txt"))
			file = fin.ReadToEnd();
		string[] lines = file.Trim().Split('\n');

		triangulationTable = new int[lines.Length][];
		for (int tti = 0; tti < triangulationTable.Length; ++tti)
			triangulationTable[tti] = JsonUtility.FromJson<ArrayWrapper<int>>(lines[tti]).items;
	}

	// Create Polyhedron
	private void GeneratePolyhedron()
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh = new Mesh();
		mesh.name = "MarchingPolyhedron";

		AddEdge(0, new Vector3(0.5773502691896257f, -0.5773502691896257f, 0.0f), 1);
		AddEdge(0, new Vector3(0.0f, -0.5773502691896257f, 0.5773502691896257f), 3);
		AddEdge(0, new Vector3(0.5773502691896257f, 0.0f, 0.5773502691896257f), 4);
		AddEdge(1, new Vector3(0.0f, -0.5773502691896257f, -0.5773502691896257f), 2);
		AddEdge(1, new Vector3(0.5773502691896257f, 0.0f, -0.5773502691896257f), 5);
		AddEdge(2, new Vector3(-0.5773502691896257f, -0.5773502691896257f, 0.0f), 3);
		AddEdge(2, new Vector3(-0.5773502691896257f, 0.0f, -0.5773502691896257f), 6);
		AddEdge(3, new Vector3(-0.5773502691896257f, 0.0f, 0.5773502691896257f), 7);
		AddEdge(4, new Vector3(0.5773502691896257f, 0.5773502691896257f, 0.0f), 5);
		AddEdge(4, new Vector3(0.0f, 0.5773502691896257f, 0.5773502691896257f), 7);
		AddEdge(5, new Vector3(0.0f, 0.5773502691896257f, -0.5773502691896257f), 6);
		AddEdge(6, new Vector3(-0.5773502691896257f, 0.5773502691896257f, 0.0f), 7);

		AddVertex(new Vector3(0.5773502691896257f, -0.5773502691896257f, 0.5773502691896257f));
		AddVertex(new Vector3(0.5773502691896257f, -0.5773502691896257f, -0.5773502691896257f));
		AddVertex(new Vector3(-0.5773502691896257f, -0.5773502691896257f, -0.5773502691896257f));
		AddVertex(new Vector3(-0.5773502691896257f, -0.5773502691896257f, 0.5773502691896257f));
		AddVertex(new Vector3(0.5773502691896257f, 0.5773502691896257f, 0.5773502691896257f));
		AddVertex(new Vector3(0.5773502691896257f, 0.5773502691896257f, -0.5773502691896257f));
		AddVertex(new Vector3(-0.5773502691896257f, 0.5773502691896257f, -0.5773502691896257f));
		AddVertex(new Vector3(-0.5773502691896257f, 0.5773502691896257f, 0.5773502691896257f));

		// Assign edges to vertices
		for (int vi = 0; vi < vertexObjects.Count; ++vi)
		{
			List<int> edges = new List<int>();
			for (int ei = 0; ei < edgeObjects.Count; ++ei)
			{
				int endOne = edgeObjects[ei].GetComponent<MarchingEdge>().endOne;
				int endTwo = edgeObjects[ei].GetComponent<MarchingEdge>().endTwo;
				if (vi == endOne || vi == endTwo)
					vertexObjects[vi].GetComponent<MarchingVertex>().edges.Add(ei, edgeObjects[ei]);
			}
		}
	}

	private void AddEdge(int endOne, Vector3 midpoint, int endTwo)
	{
		GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		edge.transform.parent = transform;
		edge.AddComponent<MarchingEdge>();
		edge.GetComponent<MarchingEdge>().Initialize(edgeObjects.Count, endOne, midpoint, endTwo);
		edgeObjects.Add(edge);
		geometricEdges.Add(midpoint);
	}

	private void AddVertex(Vector3 pos)
	{
		GameObject vertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		vertex.transform.parent = transform;
		vertex.AddComponent<MarchingVertex>();
		vertex.GetComponent<MarchingVertex>().Initialize(vertexObjects.Count, pos);
		vertexObjects.Add(vertex);
		geometricVertices.Add(pos);
	}

	// Calculate Mesh
	public /*IEnumerator*/ void CalculateMesh()
	{
		List<List<int>> edgeLoops = CalculateEdgeLoops();
		List<List<List<int>>> globalTriangles = new List<List<List<int>>>();

		int vertexCount = 0;
		int triangleCount = 0;
		string foo = "";
		for (int eli = 0; eli < edgeLoops.Count; ++eli)
		{
			// Find all permutations
			List<List<List<int>>> permutations = CalculateTrianglePermutations(edgeLoops[eli]);

			// Find correct permutation
			List<List<int>> correct = permutations[permutations.Count-1];
			globalTriangles.Add(correct);

			vertexCount += edgeLoops[eli].Count;
			triangleCount += correct.Count;

			// Debug
			string bar = permutations.Count.ToString()+" "+vertexCount.ToString()+"\n";
			for (int ii = 0; ii < permutations.Count; ++ii)
			{
				for (int jj = 0; jj < permutations[ii].Count; ++jj)
				{
					bar += "( ";
					for (int kk = 0; kk < permutations[ii][jj].Count; ++kk)
						bar += permutations[ii][jj][kk].ToString()+" ";
					bar += ") ";
				}
				bar += "\n";
			}
			Debug.Log(bar);

			for (int li = 0; li < edgeLoops[eli].Count; ++li)
				 foo += edgeLoops[eli][li]+" ";
			foo += "\n";
		}
		Debug.Log(foo);

		// If there are no vertices, then clear the mesh
		//vertexCount = (from ii in edgeLoops select ii.Count).ToList().Sum();
		if (vertexCount == 0)
		{
			GetComponent<MeshFilter>().mesh.Clear();
			return;
			// yield break;
		}

		Vector3[] meshVertices = CalculateVertices(edgeLoops, vertexCount);
		int[] meshTriangles = new int[3*triangleCount];

		foo = "";
		for (int gti = 0, mti = 0; gti < globalTriangles.Count; ++gti)
		{
			for (int gtli = 0; gtli < globalTriangles[gti].Count; ++gtli)
			{
				globalTriangles[gti][gtli] = CorrectTriangleChirality(globalTriangles[gti][gtli]);
				for (int ti = 0; ti < globalTriangles[gti][gtli].Count; ++ti, ++mti)
				{
					meshTriangles[mti] = Array.FindIndex(meshVertices, ii => ii == geometricEdges[globalTriangles[gti][gtli][ti]]);
					foo += meshTriangles[mti].ToString()+" ";
				}
			}
		}
		Debug.Log(foo);

		SetMesh(meshVertices, meshTriangles);

		/*
		Vector3[] meshVertices = CalculateVertices(edgeLoops, vertexCount);
		int[] meshTriangles = CalculateTriangles(edgeLoops, vertexCount, meshVertices);
		SetMesh(meshVertices, meshTriangles);
		//yield return new WaitForSeconds(1.0f);
		*/
	}

	private Vector3[] CalculateVertices(List<List<int>> edgeLoops, int vertexCount)
	{
		Vector3[] meshVertices = new Vector3[vertexCount];
		for (int eli = 0, vi = 0; eli < edgeLoops.Count; ++eli)
			for (int li = 0; li < edgeLoops[eli].Count; ++li, ++vi)
				meshVertices[vi] = geometricEdges[edgeLoops[eli][li]];
		return meshVertices;
	}

	private List<List<List<int>>> CalculateTrianglePermutations(List<int> meshVertexIndices)
	{
		List<List<List<int>>> permutations = new List<List<List<int>>>();
		if (meshVertexIndices.Count < 3)
			return permutations;

		List<int> segment = new List<int>(meshVertexIndices);
		int secondVertexIndex = segment[1];
		segment.RemoveAt(1);
		int firstVertexIndex = segment[0];
		segment.RemoveAt(0);

		for (int third = 0; third < segment.Count; ++third)
		{
			List<List<List<int>>> newPermutations = new List<List<List<int>>>();
			List<List<int>> prefix = new List<List<int>>();
			prefix.Add(new List<int>() {
				firstVertexIndex,
				secondVertexIndex,
				segment[third],
			});

			// Get Segments
			List<int> firstSegment = new List<int>(segment.Skip(third));
			firstSegment.Insert(0, firstVertexIndex);
			if (firstSegment.Count == 3)
				prefix.Add(firstSegment);
			List<int> secondSegment = new List<int>(segment.Take(third+1));
			secondSegment.Insert(0, secondVertexIndex);
			if (secondSegment.Count == 3)
				prefix.Add(secondSegment);

			// Check if segments need to be reduced
			List<List<List<int>>> firstPermutations = new List<List<List<int>>>();
			List<List<List<int>>> secondPermutations = new List<List<List<int>>>();
			if (firstSegment.Count > 3)
				firstPermutations = CalculateTrianglePermutations(firstSegment);
			if (secondSegment.Count > 3)
				secondPermutations = CalculateTrianglePermutations(secondSegment);

			// Finalize new permutations
			if (firstSegment.Count <= 3 && secondSegment.Count <= 3)
				newPermutations.Add(prefix);
			else if (firstSegment.Count > 3 && secondSegment.Count > 3)
				for (int fpi = 0; fpi < firstPermutations.Count; ++fpi)
					for (int spi = 0; spi < secondPermutations.Count; ++spi)
					{
						List<List<int>> item = new List<List<int>>(prefix);
						item.AddRange(firstPermutations[fpi]);
						item.AddRange(secondPermutations[spi]);
						newPermutations.Add(item);
					}
			else
			{
				newPermutations.AddRange(firstPermutations);
				newPermutations.AddRange(secondPermutations);
				for (int npi = 0; npi < newPermutations.Count; ++npi)
				{
					List<List<int>> item = new List<List<int>>(prefix);
					item.AddRange(newPermutations[npi]);
					newPermutations[npi] = item;
				}
			}
			permutations.AddRange(newPermutations);
		}
		return permutations;
	}

	private void SetMesh(Vector3[] meshVertices, int[] meshTriangles)
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		mesh.vertices = meshVertices;
		mesh.triangles = meshTriangles;
		mesh.RecalculateNormals();
	}

	// Calculate Edge Loops
	private List<List<int>> CalculateEdgeLoops()
	{
		// Get all vertices that are filled
		List<int> geoVertexIndices = new List<int>();
		for (int ii = 0; ii < vertexObjects.Count; ++ii)
			if (vertexObjects[ii].GetComponent<MarchingVertex>().filled == true)
				geoVertexIndices.Add(ii);

		// If half or more of the vertices are filled, then get the vertices that are not filled
		if (geoVertexIndices.Count >= vertexObjects.Count/2)
			geoVertexIndices = Enumerable.Range(0, vertexObjects.Count).ToList().Except(geoVertexIndices).ToList();

		List<List<int>> edgeLoops = new List<List<int>>();

		// Continue until every vertex has been explored
		while (geoVertexIndices.Count > 0)
		{
			List<int> loop = new List<int>();
			Queue<int> vertexQueue = new Queue<int>();
			vertexQueue.Enqueue(geoVertexIndices[0]);
			while (vertexQueue.Count > 0)
			{
				int vertex = vertexQueue.Dequeue();
				// Removing the vertex counts it as explored
				geoVertexIndices.Remove(vertex);
				// Check along each edge coming from the current vertex for a filled vertex
				foreach (KeyValuePair<int, GameObject> entry in vertexObjects[vertex].GetComponent<MarchingVertex>().edges)
				{
					MarchingEdge edge = entry.Value.GetComponent<MarchingEdge>();
					if (edge.GetComponent<Renderer>().enabled)
					{
						// Only add the vertex if it hasn't been added before
						if (!loop.Contains(entry.Key))
							loop.Add(entry.Key);
					}
					else // If the edge is not enabled, then it connects to another vertex that should be explored
					{
						// Get the vertex on this edge that is not the current vertex
						int nextVertex = edge.endOne == vertex ? edge.endTwo : edge.endOne;
						// Check if the next vertex has already been explored
						if (geoVertexIndices.Contains(nextVertex))
							vertexQueue.Enqueue(nextVertex);
					}
				}
			}
			edgeLoops.Add(UntwistLoop(loop));
		}

		return edgeLoops;
	}

	private List<int> UntwistLoop(List<int> loop)
	{
		// Make sure the loop is large enough to be twisted
		if (loop.Count <= 3)
			return loop;

		List<int> untwistedLoop = new List<int>();
		int offset = 0;
		bool ambiguous = false;
		do
		{
			ambiguous = false;
			untwistedLoop.Clear();

			List<int> shiftedLoop = ShiftList(loop, offset);
			untwistedLoop.Add(shiftedLoop[0]);
			shiftedLoop.RemoveAt(0);
			// Continue until each vertex in the shifted loop has been reordered
			while (shiftedLoop.Count > 0)
			{
				Vector3 currentVertex = geometricEdges[untwistedLoop.Last()];
				int nextVertexIndex = shiftedLoop[0];
				for (int ii = 1; ii < shiftedLoop.Count; ++ii)
				{
					float nextDistance = Vector3.Distance(geometricEdges[nextVertexIndex], currentVertex);
					float otherDistance = Vector3.Distance(geometricEdges[shiftedLoop[ii]], currentVertex);
					// If the next vertex in the shifted loop is further away
					// from the current vertex than the other vertex, then the
					// other vertex should be added to the untwisted loop
					if (nextDistance > otherDistance)
						nextVertexIndex = shiftedLoop[ii];
					// If there are two distinct vertices that are the same
					// distance away from the current vertex, then there is more
					// than one way to untwist the loop
					else if (nextDistance == otherDistance)
					{
						++offset;
						ambiguous = true;
					}
				}
				untwistedLoop.Add(nextVertexIndex);
				shiftedLoop.Remove(nextVertexIndex);
			}
		// Continue until an unambigous way to untwist the loop has been found
		// or until offsetting the loop again would result in the original loop
		} while (offset < loop.Count && ambiguous);

		return untwistedLoop;
	}

	private List<T> ShiftList<T>(List<T> original, int offset)
	{
		List<T> shifted = new List<T>();
		for (int ii = 0; ii < original.Count; ++ii)
			shifted.Add(original[(ii+offset)%original.Count]);
		return shifted;
	}

	private List<int> CorrectTriangleChirality(List<int> triangle /* global indices */)
	{
		Vector3 edgeA = geometricEdges[triangle[0]];
		Vector3 edgeB = geometricEdges[triangle[1]];
		Vector3 edgeC = geometricEdges[triangle[2]];

		// Get filled vertex position
		MarchingEdge edge = edgeObjects[triangle[1]].GetComponent<MarchingEdge>();
		int vi =  vertexObjects[edge.endOne].GetComponent<MarchingVertex>().filled ? edge.endOne : edge.endTwo;
		Vector3 filledVertex = geometricVertices[vi]-new Vector3(
			(edgeA.x+edgeB.x+edgeC.x)/3,
			(edgeA.y+edgeB.y+edgeC.y)/3,
			(edgeA.z+edgeB.z+edgeC.z)/3
		);

		Vector3 normal = Vector3.Cross(edgeB-edgeA, edgeC-edgeA).normalized;
		float dot = Vector3.Dot(normal, filledVertex);
		// If the dot product is less than 1, then flip edges B and C
		List<int> correctTriangle = new List<int>();
		correctTriangle.Add(triangle[0]);
		correctTriangle.Add(dot < 0 ? triangle[1] : triangle[2]);
		correctTriangle.Add(dot < 0 ? triangle[2] : triangle[1]);
		return correctTriangle;
	}

	// Depreciated
	private int[] CalculateTriangles(List<List<int>> edgeLoops, int vertexCount, Vector3[] meshVertices)
	{
		int[] meshTris = new int[3*(vertexCount-2*edgeLoops.Count)];
		for (int eli = 0, ti = 0; eli < edgeLoops.Count; ++eli)
		{
			List<int> loop = new List<int>(edgeLoops[eli]);
			while (loop.Count >= 3)
			{
				Vector3 edgeA = geometricEdges[loop[0]];
				Vector3 edgeB = geometricEdges[loop[1]];
				Vector3 edgeC = geometricEdges[loop[2]];

				// Get filled vertex position
				MarchingEdge edge = edgeObjects[loop[1]].GetComponent<MarchingEdge>();
				int vi =  vertexObjects[edge.endOne].GetComponent<MarchingVertex>().filled ? edge.endOne : edge.endTwo;
				Vector3 filledVertex = geometricVertices[vi]-new Vector3(
					(edgeA.x+edgeB.x+edgeC.x)/3,
					(edgeA.y+edgeB.y+edgeC.y)/3,
					(edgeA.z+edgeB.z+edgeC.z)/3
				);

				Vector3 normal = Vector3.Cross(edgeB-edgeA, edgeC-edgeA).normalized;
				float dot = Vector3.Dot(normal, filledVertex);
				// If the dot product is less than 1, then flip edges B and C
				meshTris[ti] = Array.FindIndex(meshVertices, ii => ii == edgeA);
				meshTris[ti+1] = Array.FindIndex(meshVertices, ii => ii == (dot < 0 ? edgeB : edgeC));
				meshTris[ti+2] = Array.FindIndex(meshVertices, ii => ii == (dot < 0 ? edgeC : edgeB));

				loop.RemoveAt(1);
				ti += 3;
			}
		}
		return meshTris;
	}
}

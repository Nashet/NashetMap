using Nashet.MeshData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.MapMeshes
{
	public class ProvinceMesh : IProvinceMesh
	{
		private static readonly Dictionary<int, ProvinceMesh> lookUp = new Dictionary<int, ProvinceMesh>();

		public int ID { get; protected set; }

		public GameObject GameObject { get; protected set; }
		public Vector3 Position { get; protected set; }

		protected MeshFilter MeshFilter { get; private set; }

		protected MeshRenderer meshRenderer;

		private readonly Dictionary<int, MeshRenderer> bordersMeshes = new Dictionary<int, MeshRenderer>();
		private Material defaultMaterial;


		public ProvinceMesh(int ID, MeshStructure meshStructure, Dictionary<int, MeshStructure> neighborBorders,
			Color provinceColor, Transform parent, Material defaultBorderMaterial, Material material, string name = "")
		{
			this.ID = ID;
			defaultMaterial = material;

			//spawn object
			GameObject = new GameObject(string.Format("{0} {1}", ID, name));

			//Add Components
			MeshFilter = GameObject.AddComponent<MeshFilter>();
			meshRenderer = GameObject.AddComponent<MeshRenderer>();

			// in case you want the new gameobject to be a child
			// of the gameobject that your script is attached to
			GameObject.transform.parent = parent;

			var landMesh = MeshFilter.mesh;

			landMesh.vertices = meshStructure.getVertices().ToArray(); //make sure memeory is freeded
			landMesh.triangles = meshStructure.getTriangles().ToArray();

			// next line causes out of memory error in web GL
			landMesh.uv = SetUV(meshStructure.getVertices()); // dont use getUVmap()

			landMesh.RecalculateNormals();
			landMesh.RecalculateBounds();
			landMesh.name = ID.ToString();

			Position = SetProvinceCenter(meshStructure);// I can use mesh.bounds.center, but it will center off a little bit


			MeshCollider groundMeshCollider = GameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
			groundMeshCollider.sharedMesh = MeshFilter.mesh;

			meshRenderer.sharedMaterial = material;

			meshRenderer.sharedMaterial.color = provinceColor;

			CreateBorderMeshes(neighborBorders, defaultBorderMaterial);
			lookUp.Add(ID, this);

			landMesh.Optimize();
			landMesh.UploadMeshData(true);
		}

		private void CreateBorderMeshes(Dictionary<int, MeshStructure> neighborBorders, Material defaultBorderMaterial)
		{
			foreach (var border in neighborBorders)
			{
				var neighbor = border.Key;
				{

					GameObject borderObject = new GameObject($"Border with {neighbor}");

					//Add Components
					MeshFilter = borderObject.AddComponent<MeshFilter>();
					MeshRenderer meshRenderer = borderObject.AddComponent<MeshRenderer>();

					borderObject.transform.parent = GameObject.transform;

					Mesh borderMesh = MeshFilter.mesh;
					borderMesh.vertices = border.Value.getVertices().ToArray();
					borderMesh.triangles = border.Value.getTriangles().ToArray();
					borderMesh.uv = border.Value.getUVmap().ToArray(); //todo dont generate UV if not needed
					borderMesh.RecalculateNormals();
					borderMesh.RecalculateBounds();
					meshRenderer.material = defaultBorderMaterial;
					borderMesh.Optimize();
					borderMesh.UploadMeshData(true);

					bordersMeshes.Add(neighbor, meshRenderer);
				}
			}
		}

		public void SetColor(Color color)
		{
			if (meshRenderer.sharedMaterial != defaultMaterial)
				meshRenderer.sharedMaterial = defaultMaterial;
			meshRenderer.sharedMaterial.color = color;
		}

		public void SetMaterial(Material material) => meshRenderer.sharedMaterial = material;

		public void SetBorderMaterial(int id, Material material)
		{
			bordersMeshes[id].material = material;
		}

		private static Vector3 SetProvinceCenter(MeshStructure meshStructure)
		{
			float x = 0;
			float y = 0f;
			float z = 0f;
			List<Vector3> list = meshStructure.getVertices();
			for (int i = 0; i < list.Count; i++)
			{
				x += list[i].x;
				y += list[i].y;
				z += list[i].z;
			}

			var accumulator = new Vector3(x, y, z) / meshStructure.verticesCount;
			return accumulator;
		}

		private static Vector2[] SetUV(List<Vector3> vertices)
		{
			var uvCoordinates = new Vector2[vertices.Count];

			for (int i = 0; i < vertices.Count; i++)
			{
				uvCoordinates[i].x = vertices[i].x;
				uvCoordinates[i].x = vertices[i].y;
			}

			return uvCoordinates;
		}

		public static ProvinceMesh GetById(int id)
		{
			return lookUp[id];
		}

		public static int? GetIdByCollider(Collider collider)
		{
			if (collider != null)
			{
				MeshCollider meshCollider = collider as MeshCollider;
				if (meshCollider == null || meshCollider.sharedMesh == null)
					return null;

				Mesh mesh = meshCollider.sharedMesh;

				if (mesh.name == "Quad") // here you can filter out non-province meshes //todo remove it
					return null;

				int provinceNumber = Convert.ToInt32(mesh.name);
				return provinceNumber;
			}
			else
				return null;
		}
	}
}

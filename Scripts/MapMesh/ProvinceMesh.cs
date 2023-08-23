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
			Color provinceColor, Transform parent, Material defaultBorderMaterial, string name = "", Material material = null)
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

			landMesh.vertices = meshStructure.getVertices().ToArray();
			landMesh.triangles = meshStructure.getTriangles().ToArray();
			SetUV(landMesh); // dont use getUVmap()

			landMesh.RecalculateNormals();
			landMesh.RecalculateBounds();
			landMesh.name = ID.ToString();
			landMesh.Optimize();
			landMesh.UploadMeshData(true);

			Position = setProvinceCenter(meshStructure);// I can use mesh.bounds.center, but it will center off a little bit


			MeshCollider groundMeshCollider = GameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
			groundMeshCollider.sharedMesh = MeshFilter.mesh;

			meshRenderer.material = material;

			meshRenderer.material.color = provinceColor;


			//making meshes for border
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
					borderMesh.uv = border.Value.getUVmap().ToArray();
					borderMesh.RecalculateNormals();
					borderMesh.RecalculateBounds();
					meshRenderer.material = defaultBorderMaterial;
					borderMesh.Optimize();
					borderMesh.UploadMeshData(true);

					bordersMeshes.Add(neighbor, meshRenderer);
				}
			}
			lookUp.Add(ID, this);
		}

		public void SetColor(Color color)
		{
			if (meshRenderer.material != defaultMaterial)
				meshRenderer.material = defaultMaterial;
			meshRenderer.material.color = color;
		}

		public void SetMaterial(Material material) => meshRenderer.material = material;

		public void SetBorderMaterial(int id, Material material)
		{
			bordersMeshes[id].material = material;
		}

		private static Vector3 setProvinceCenter(MeshStructure meshStructure)
		{
			Vector3 accu = new Vector3(0, 0, 0);
			foreach (var c in meshStructure.getVertices())
				accu += c;
			accu = accu / meshStructure.verticesCount;
			return accu;
		}

		private static void SetUV(Mesh landMesh)
		{
			Vector2[] uvCoordinates = new Vector2[landMesh.vertices.Length];

			for (int i = 0; i < landMesh.vertices.Length; i++)
			{
				uvCoordinates[i] = new Vector2(landMesh.vertices[i].x, landMesh.vertices[i].y);
			}

			landMesh.uv = uvCoordinates;
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

				if (mesh.name == "Quad") // here you can filter out non-province meshes
					return null;

				int provinceNumber = Convert.ToInt32(mesh.name);
				return provinceNumber;
			}
			else
				return null;
		}
	}
}

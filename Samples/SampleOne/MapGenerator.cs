﻿using Nashet.MarchingSquares;
using Nashet.MeshData;
using Nashet.NameGeneration;
using Nashet.Map.Utils;
using Nashet.MapMeshes;
using Nashet.Map.GameplayControllers;
using System.Collections.Generic;
using UnityEngine;
using QPathFinder;
using System.Linq;

namespace Nashet.Map.Examples
{
	public class MapGenerator : MonoBehaviour
	{
		public bool IsReady { get; private set; }
		[SerializeField] Material provinceShoreMaterial;
		[SerializeField] Material defaultProvinceBorderMaterial;
		[SerializeField] Material riverMaterial;
		[SerializeField] Material impassableBorderMaterial;
		[SerializeField] Material defaultCountryBorderMaterial;

		[SerializeField] GameObject r3DProvinceTextPrefab;
		[SerializeField] GameObject r3DCountryTextPrefab;
		[SerializeField] CameraController cameraController;

		private Dictionary<int, Dictionary<int, MeshStructure>> meshes;

		private float cellMultiplier = 1f;
		private int amountOFCountries = 19;
		private int riverLenght = 16;
		private int maxRiversAmount = 8;
		private int removeSmallProvincesLimit = 50;
		private int lakechance = 20;

		private void Start()
		{
			GenerateWorld();
		}

		public Rect GenerateWorld()
		{
			var mapTexture = PprepareTexture(null);
			var mapBorders = new Rect(0f, 0f, mapTexture.getWidth() * cellMultiplier, mapTexture.getHeight() * cellMultiplier);
			var grid = new VoxelGrid(mapTexture.getWidth(), mapTexture.getHeight(), cellMultiplier * mapTexture.getWidth(), mapTexture);

			meshes = new Dictionary<int, Dictionary<int, MeshStructure>>();

			cameraController.Initialize(mapBorders);

			CreateProvinces(mapTexture, grid);

			SetNeighbors();

			AddMountains();
			AddRivers();

			SetPatchFinding(meshes);

			CreateCountries();
			GiveExtraProvinces();
			UpdateCountryBordersMaterials();

			return mapBorders;
		}

		private void UpdateCountryBordersMaterials()
		{
			foreach (var province in Province.AllProvinces.Values)
			{
				foreach (var border in province.neughbors)
				{
					var neighbor = border.Province;
					if (!border.IsPassable || border.IsRiverBorder)
					{
						continue;
					}

					if (province.Country == neighbor.Country)
					{
						province.provinceMesh.SetBorderMaterial(neighbor.Id, defaultProvinceBorderMaterial);
						neighbor.provinceMesh.SetBorderMaterial(province.Id, defaultProvinceBorderMaterial);
						continue;
					}

					if (province.Country == null)
					{
						province.provinceMesh.SetBorderMaterial(neighbor.Id, defaultProvinceBorderMaterial);
					}
					else
					{
						province.provinceMesh.SetBorderMaterial(neighbor.Id, (province.Country as Country).borderMaterial);
					}

					if (neighbor.Country == null)
					{
						neighbor.provinceMesh.SetBorderMaterial(province.Id, defaultProvinceBorderMaterial);
					}
					else
					{
						neighbor.provinceMesh.SetBorderMaterial(province.Id, (neighbor.Country as Country).borderMaterial);
					}
				}
			}
		}

		public static void GiveExtraProvinces()
		{
			var howMuchGive = 4;
			for (int i = 0; i < howMuchGive; i++)
			{
				foreach (var item in Country.AllCountries)
				{
					var around = item.Capital;
					GiveRandomNeighbor(item, around);
				}
			}
			for (int i = 0; i < 2; i++)
				foreach (var item in Province.AllProvinces.Values)
				{
					if (item.Country != null)
						GiveRandomNeighbor(item.Country as Country, item);
				}
		}

		private static void GiveRandomNeighbor(Country item, Province around)
		{
			var neighbors = around.neughbors.Where(x => x.Province.Country == null && x.IsPassable).ToList();
			var randomElement = Random.Range(0, neighbors.Count);
			var emptyProvince = neighbors.Count == 0 ? null : neighbors[randomElement].Province;
			if (emptyProvince != null)
			{
				AnnexProvince(item, emptyProvince);
			}
		}

		private static void AnnexProvince(Country country, Province emptyProvince)
		{
			emptyProvince.provinceMesh.SetColor(country.NationalColor.getAlmostSameColor());
			emptyProvince.Country = country;
		}

		private void CreateProvinces(MyTexture texture, VoxelGrid grid)
		{
			var borderProvinces = texture.GetColorsFromBorder();
			foreach (var colorId in texture.AllUniqueColors3())
			{

				var makeSea = borderProvinces.Contains(colorId) || Rand.Chance(lakechance);
				if (makeSea)
					continue;

				var size = texture.CountPixels(colorId);
				if (size < removeSmallProvincesLimit)
					continue;

				var name = ProvinceNameGenerator.generateWord(6);
				var id = colorId.ToInt();
				var meshStructure = grid.getMesh(id, out var borderMeshes);
				var provinceMesh = new ProvinceMesh(id, meshStructure, borderMeshes, Color.yellow,
					this.transform, provinceShoreMaterial, name);

				provinceMesh.SetMaterial(new Material(Shader.Find("Standard")));

				var label = MapTextLabel.CreateMapTextLabel(r3DProvinceTextPrefab, provinceMesh.Position, name, Color.black); //walletComponent.name
				label.transform.SetParent(provinceMesh.GameObject.transform, false);

				var province = new Province(id, name);
				var node = new Node(provinceMesh.Position, province);
				//node.autoGeneratedID = id;
				PathFinder.instance.graphData.nodes.Add(node);

				province.Node = node;
				province.Position = provinceMesh.Position;
				province.provinceMesh = provinceMesh;
				meshes.Add(id, borderMeshes);
			}
		}

		private void SetNeighbors()
		{
			foreach (var province in Province.AllProvinces)
			{
				foreach (var neighborId in meshes[province.Key])
				{
					Province.AllProvinces.TryGetValue(neighborId.Key, out var neighbor);
					if (neighbor == null)
						continue;
					province.Value.neughbors.Add(new Border(neighbor));
				}
			}
		}

		private void AddMountains()
		{
			foreach (var item in Province.AllProvinces.Values)
			{
				foreach (var border in item.neughbors)
				{
					if (item.Terrain == Province.TerrainTypes.Mountains && border.Province.Terrain == Province.TerrainTypes.Mountains)
					{
						border.IsPassable = false;
						item.SetBorderMaterial(border.Province.Id, impassableBorderMaterial);
					}
				}
			}
		}

		private void CreateCountries()
		{
			for (int i = 0; i < amountOFCountries; i++)
			{
				var name = CountryNameGenerator.generateCountryName();
				var color = ColorExtensions.getRandomColor();
				var country = new Country(color, name, defaultCountryBorderMaterial);
				//countriesLookup.Add(world.PackEntity(entity));
				var random = Random.Range(0, Province.AllProvinces.Count - 1);
				var capital = Province.AllProvinces.ElementAt(random);
				var meshCapitalText = MapTextLabel.CreateMapTextLabel(r3DCountryTextPrefab, capital.Value.Position,
					name, Color.cyan, 32);
				meshCapitalText.transform.SetParent(this.gameObject.transform);
				capital.Value.Country = country;
				country.Capital = capital.Value;
				AnnexProvince(country, capital.Value);
			}
		}

		private static void SetPatchFinding(Dictionary<int, Dictionary<int, MeshStructure>> meshes)
		{
			PathFinder.instance.graphData.ReGenerateIDs();

			foreach (var province in Province.AllProvinces)
			{
				foreach (var border in province.Value.neughbors)
				{
					if (!border.IsPassable)
						continue;

					if (PathFinder.instance.graphData.paths.Exists(x => x.IDOfA == province.Value.Node.autoGeneratedID && x.IDOfB == border.Province.Node.autoGeneratedID))
						continue;

					PathFinder.instance.graphData.paths.Add(
						new Path(border.Province.Node.autoGeneratedID, province.Value.Node.autoGeneratedID));
				}
			}
		}

		private void AddRivers()
		{
			for (int i = 0; i < maxRiversAmount; i++)
			{
				var random = Random.Range(0, Province.AllProvinces.Count);
				var riverStart = Province.AllProvinces.Values.Where(x => !x.neughbors.Any(y => y.Province.isRiverNeighbor(x))).ElementAtOrDefault(random);//
																																						  //x.IsCoastal && 
																																						  //x.Terrain == Province.TerrainTypes.Mountains &&
				if (riverStart == null)
					continue;
				var random2 = Random.Range(0, riverStart.neughbors.Count);
				var riverStart2 = riverStart.neughbors.ElementAtOrDefault(random2).Province;
				//.Where(x => x.IsCoastal)
				if (riverStart2 == null)
					continue;
				AddRiverBorder(riverStart, riverStart2);
			}
		}

		private void AddRiverBorder(Province beach1, Province beach2)
		{
			var logRivers = true;
			if (beach1.Terrain == Province.TerrainTypes.Mountains && beach2.Terrain == Province.TerrainTypes.Mountains)
			{
				if (logRivers)
					Debug.Log($"----river stoped because of mountain");
				return;
			}

			var chanceToContinue = Rand.Get.Next(riverLenght);
			if (chanceToContinue == 1)
			{
				if (logRivers)
					Debug.Log($"----river stoped because its long enough");
				return;
			};

			Province beach3 = null;

			var potentialBeaches = beach1.neughbors.Where(x => x.Province.isNeighbor(beach2)).ToList();
			{

				if (potentialBeaches.Count == 1)
				{
					beach3 = potentialBeaches.ElementAt(0).Province;
					if (beach3.isRiverNeighbor(beach1) || beach3.isRiverNeighbor(beach2))
					{
						beach3 = null;
					}
				}

				if (potentialBeaches.Count == 2)
				{
					var chooseBeach = Rand.Get.Next(2);
					if (chooseBeach == 0)
					{
						beach3 = potentialBeaches.ElementAt(0).Province;
						if (beach3.isRiverNeighbor(beach1) || beach3.isRiverNeighbor(beach2))
						{
							beach3 = potentialBeaches.ElementAt(1).Province;
						}
					}
					if (chooseBeach == 1)
					{
						beach3 = potentialBeaches.ElementAt(1).Province;
						if (beach3.isRiverNeighbor(beach1) || beach3.isRiverNeighbor(beach2))
						{
							beach3 = potentialBeaches.ElementAt(0).Province;
						}
					}
				}
			}
			if (logRivers)
				Debug.Log($"{beach1}, {beach2}");

			//meshes[beach1.Id][beach2.Id]

			beach1.AddRiverBorder(beach2, riverMaterial);
			beach2.AddRiverBorder(beach1, riverMaterial);

			var chance = Rand.Get.Next(2);

			if (beach3 == null)
			{
				if (logRivers)
					Debug.Log($"----river stoped because cant find beach3");
				return;
			};

			if (chance == 1 && !beach3.isRiverNeighbor(beach1))
			{
				AddRiverBorder(beach3, beach1);
			}
			else
			{
				AddRiverBorder(beach3, beach2);
			}
		}

		private MyTexture PprepareTexture(Texture2D mapImage)
		{
			MyTexture mapTexture;

			if (mapImage == null)
			{

				var height = 130;
				var width = 150 + Rand.Get.Next(30);

				height = 230;
				width = 250 + Rand.Get.Next(60);

				//height = 430;
				//width = 450 + Rand.Get.Next(60);

				int amountOfProvince = height * width / 140 + Rand.Get.Next(5);
				var map = new MapTextureGenerator();
				mapTexture = map.generateMapImage(width, height, amountOfProvince);
			}
			else
			{
				//Texture2D mapImage = Resources.Load("provinces", typeof(Texture2D)) as Texture2D; ///texture;
				mapTexture = new MyTexture(mapImage);
			}
			return mapTexture;
		}
	}
}
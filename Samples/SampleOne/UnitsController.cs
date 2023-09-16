﻿using Nashet.FlagGeneration;
using Nashet.GameplayView;
using Nashet.Map.Utils;
using Nashet.UnitSelection;
using QPathFinder;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.Map.Examples
{
	public class UnitsController : MonoBehaviour
	{
		[SerializeField] UnitView unitPrefab;
		[SerializeField] MapGenerator mapGenerator;
		[SerializeField] private int initialPoolSize = 10;

		private MonoObjectPool<UnitView> unitPool;
		private Dictionary<Collider, UnitView> unitsLookup = new();

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => mapGenerator.IsReady);
			Initialize();
		}

		private void Initialize()
		{
			unitPool = new MonoObjectPool<UnitView>(unitPrefab, initialPoolSize);
			var cameraHolder = Camera.main;

			var selectionComponent = cameraHolder.gameObject.GetComponent<SelectionComponent>();
			selectionComponent.OnEntityClicked += UnitClickedHandler;

			var provinceSelection = cameraHolder.GetComponent<ProvinceSelectionHelper>();
			provinceSelection.ProvinceSelected += ProvinceSelectedHandler;

			CreateUnitsForTest();
		}

		private void ProvinceSelectedHandler(Province targetProvince)
		{
            if (targetProvince == null)
            {
				return;
            }

            foreach (var unit in unitsLookup.Values)
			{
				if (!unit.IsSelected)
					continue;

				var province = Province.AllProvinces[unit.ProvinceId];
				PathFinder.instance.FindShortestPathOfNodes(province.Node.autoGeneratedID, targetProvince.Node.autoGeneratedID, Execution.Synchronous, (path) =>
			{
				unit.SetPath(path);
			});
			}
		}

		private void UnitClickedHandler(SelectionData data)
		{
			if (data == null || data.MultipleSelection != null)
				return; // for now
			if (unitsLookup.TryGetValue(data.SingleSelection, out var unit))
			{
				if (unit.IsSelected)
				{
					unit.Deselect();
				}
				else
				{
					unit.Select();
				}
			}
		}

		private void CreateUnitsForTest()
		{
			foreach (var item in Country.AllCountries)
			{
				var texture = FlagGenerator.Generate(128, 128);
				Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(1f, 1f));
				var unit = unitPool.Get();
				unit.gameObject.transform.SetParent(transform);
				unit.Initialize(item.Capital.Position, item.Capital.Id, sprite);
				unitsLookup.Add(unit.GetCollider(), unit);
			}
		}
	}
}
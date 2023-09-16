using Nashet.FlagGeneration;
using Nashet.GameplayView;
using Nashet.Map.Utils;
using Nashet.UnitSelection;
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
			var selectionComponent = Camera.main.gameObject.GetComponent<SelectionComponent>();
			selectionComponent.OnEntityClicked += UnitClickedHandler;
			CreateUnitsForTest();
		}

		private void UnitClickedHandler(SelectionData data)
		{
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
				unit.Initialize(item.Capital.Position, sprite);
				unitsLookup.Add(unit.GetCollider(), unit);
			}
		}
	}
}
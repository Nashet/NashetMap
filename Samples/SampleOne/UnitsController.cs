using Nashet.FlagGeneration;
using Nashet.GameplayView;
using Nashet.Map.Utils;
using System.Collections;
using UnityEngine;

namespace Nashet.Map.Examples
{
	public class UnitsController : MonoBehaviour
	{
		[SerializeField] UnitView unitPrefab;
		[SerializeField] MapGenerator mapGenerator;
		private MonoObjectPool<UnitView> unitPool;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => mapGenerator.IsReady);
			unitPool = new MonoObjectPool<UnitView>(unitPrefab, 6);

			CreateUnitsForTest();
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
			}
		}
	}
}
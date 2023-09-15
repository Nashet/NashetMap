using System.Collections.Generic;
using UnityEngine;

namespace Nashet.Map.Utils
{
	public class MonoObjectPool<T> where T : MonoBehaviour
	{
		private readonly Queue<T> availableElements = new();
		private readonly T prefab;

		public MonoObjectPool(T prefab, int initialSize)
		{
			this.prefab = prefab;

			for (int i = 0; i < initialSize; i++)
			{
				T obj = GameObject.Instantiate(prefab);
				obj.gameObject.SetActive(false);
				availableElements.Enqueue(obj);
			}
		}

		public T Get()
		{
			if (availableElements.Count == 0)
			{
				T obj = GameObject.Instantiate(prefab);
				obj.gameObject.SetActive(true);
				return obj;
			}

			T pooledObject = availableElements.Dequeue();
			pooledObject.gameObject.SetActive(true);
			return pooledObject;
		}

		public void Return(T obj)
		{
			obj.gameObject.SetActive(false);
			availableElements.Enqueue(obj);
		}
	}
}
using System;
using System.Collections.Generic;
using UnityEngine;
using QPathFinder;

namespace Nashet.GameplayView
{
	public class UnitView : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer flag;
		[SerializeField] private GameObject selectionPart;
		[SerializeField] private LineRenderer movementDirection;
		[SerializeField] private LineRenderer pathRenderer;

		private const float enemyDirectionScale = 0.6f;

		internal void Initialize(Vector3 position, Sprite sprite)
		{
			SetFlag(sprite);
			position.z = -0.1f;
			gameObject.transform.position = position;
			Deselect();
			movementDirection.enabled = false;
		}
		private void SetFlag(Sprite sprite)
		{
			this.flag.sprite = sprite;
		}

		public void Select()
		{
			selectionPart.SetActive(true);
		}

		public void Deselect()
		{
			selectionPart.SetActive(false);
		}

		private void SetMove(List<Node> path)
		{
			pathRenderer.positionCount = path.Count + 1;
			pathRenderer.SetPositions(GetAdjustedPositions(path));
			pathRenderer.SetPosition(0, transform.position);

			this.transform.LookAt(path[0].Position, Vector3.back);

			movementDirection.positionCount = 2;
			//todo must be fixed size
			var linePositions = GetAdjustedPositions(path); //top dont call twice
			linePositions[0] = transform.position;
			linePositions[1] = Vector3.LerpUnclamped(linePositions[1], linePositions[0], enemyDirectionScale);
			movementDirection.SetPositions(linePositions);

			movementDirection.enabled = true;
		}

		private static Vector3[] GetAdjustedPositions(List<Node> path)
		{
			Vector3[] array = new Vector3[path.Count + 1];
			for (int i = 0; i < path.Count; i++)
			{
				array[i + 1] = path[i].Position;
				array[i + 1].z = -2f;
			}
			return array;
		}

		private void StopMovement()
		{
			pathRenderer.positionCount = 0;

			this.transform.eulerAngles = new Vector3(270f, 0f, 0f);
			movementDirection.enabled = false;
		}
	}
}

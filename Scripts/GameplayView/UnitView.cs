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
		[SerializeField] private GameObject body;

		public bool IsSelected => selectionPart.activeSelf;

		public bool HasPath => path != null && path.Count > 0;
		public int ProvinceId { get; private set; }

		private const float enemyDirectionScale = 0.6f;

		private int movementProgress;
		private List<Node> path;

		internal void Initialize(Vector3 position, int provinceID, Sprite sprite)
		{
			this.ProvinceId = provinceID;
			SetFlag(sprite);
			position.z = -0.1f;
			gameObject.transform.position = position;
			Deselect();
			movementDirection.enabled = false;
			movementProgress = 0;
			path = null;
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

		public Collider GetCollider()
		{
			return body.GetComponent<Collider>();
		}

		public void SetPath(List<Node> path)
		{
			this.path = path;
			movementProgress = 0;
			if (path == null || path.Count == 0)
			{
				StopMovement();
				return;
			}

			pathRenderer.positionCount = path.Count + 1;
			pathRenderer.SetPositions(GetAdjustedPositions(path));
			pathRenderer.SetPosition(0, transform.position);

			LookAt(path[0]);

			movementDirection.positionCount = 2;
			//todo must be fixed size
			var linePositions = GetAdjustedPositions(path); //top dont call twice
			linePositions[0] = transform.position;
			linePositions[1] = Vector3.LerpUnclamped(linePositions[1], linePositions[0], enemyDirectionScale);
			movementDirection.SetPositions(linePositions);

			movementDirection.enabled = true;
		}

		private void LookAt(Node target)
		{
			// Get the direction from the current object to the target object
			Vector2 direction = target.Position - transform.position;

			// Calculate the angle in degrees
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

			// Apply the rotation around the Z-axis
			transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
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

			this.transform.eulerAngles = new Vector3(0f, 0f, 0f);
			movementDirection.enabled = false;
		}

		internal void Move()
		{
			movementProgress += 1;
			if (movementProgress > 10) //todo here should be cost of traveling instead of 10
			{
				ProvinceChangedHandler();
			}
		}

		private void ProvinceChangedHandler()
		{
			ProvinceId = path[0].Province.Id;
			movementProgress = 0;
			var nextPosition = path[0].Position;
			transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
			path.RemoveAt(0);
			SetPath(path);
		}
	}
}

using System;
using Managers;
using MyInterfaces;
using Player;
using Unity.Netcode;
using UnityEditor.Searcher;
using UnityEngine;

namespace World
{
	[RequireComponent(typeof(Collider))]
	public class MoveableObject : NetworkBehaviour, IInteractable, IObjectID
	{
		#region Variables

		public Action<Collider> OnObjectDisable;
		[field: SerializeField] public uint ID { get; private set; }

		public bool isOnMinigame;
		public bool beingMoved;
		private Collider _collider;

		#endregion

		#region Unity Methods

		protected virtual void Awake()
		{
			_collider = GetComponent<Collider>();
		}

		protected virtual void Update()
		{
			if (beingMoved) return;
			if (!Physics.Raycast(transform.position, Vector3.down, out var hit, float.PositiveInfinity, ~(1 << 2),
				    QueryTriggerInteraction.Ignore)) {
				return;
			}

			transform.up = Vector3.Lerp(transform.up, hit.normal, 10f * Time.deltaTime);

			var tempPos = hit.point + transform.up * _collider.bounds.extents.y;
			transform.position = Vector3.Lerp(transform.position, tempPos, 9.81f * Time.deltaTime);

			if (!hit.collider.TryGetComponent(out MoveableObject _))
				transform.parent = hit.transform;
		}

		private void OnDisable()
		{
			OnObjectDisable?.Invoke(_collider);
		}

		#endregion

		#region RPC Methods

		public void PlaceDown()
		{
			beingMoved = false;
		}

		public void MoveOnMinigame(Vector3 position)
		{
			beingMoved = true;
			SetObjectActiveRpc(true, position);
		}

		[Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
		private void SetObjectActiveRpc(bool setActive, Vector3 placePosition)
		{
			MoveLocally(setActive, placePosition);
		}

		protected virtual void MoveLocally(bool setActive, Vector3 placePosition)
		{
			transform.position = placePosition;
			gameObject.SetActive(setActive);
		}

		public void PlayPickupSound()
		{
			AudioManager.Instance.PlayOneShot(AudioEvents.Instance.itemPickup, transform.position);
		}

		#endregion

		#region Interface Methods

		public IInteractable PickupOrDropObject(bool pickUp, Vector3 placePosition = default)
		{
			SetObjectActiveRpc(!pickUp, placePosition);
			return this;
		}

		public virtual IInteractable SecondaryInteract(OwnerPlayer interactor)
		{
			return null;
		}

		public virtual string GetInteractText()
		{
			return $"Pick up {gameObject.name}";
		}

		public virtual bool IsInteractedWith()
		{
			return false;
		}

		#endregion
	}
}
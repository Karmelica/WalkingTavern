using System.Collections.Generic;
using Cooking.Minigames.Helpers;
using JetBrains.Annotations;
using Managers;
using MyInterfaces;
using Player;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using World;

namespace Cooking.Minigames
{
	[RequireComponent(typeof(Helper))]
	public abstract class Minigame : NetworkBehaviour, IInteractable
	{
		[Header("Minigame Properties")] [SerializeField]
		protected int requiredScore = 10;

		[Header("Components")] [SerializeField]
		private TextMeshProUGUI instructions;

		[SerializeField] [CanBeNull] private GameObject tool;
		public Transform cameraLocation;
		public Transform foodPlaceholder;
		protected List<MoveableObject> CurrentFood = new();
		protected bool DidHit;
		protected Helper Helper;
		protected bool Interacted;
		protected Camera MainCamera;
		protected Vector2 MousePos;
		protected OwnerPlayer OwnerPlayer;
		protected RaycastHit RayHit;
		protected int Score;
		protected bool ShowCursor = true;

		protected virtual void Awake()
		{
			MainCamera = Camera.main;
			Helper = GetComponent<Helper>();
			RayHit = new RaycastHit();
		}

		protected virtual void Update()
		{
			MoveTool();
			if (!CheckForIngredients()) return;

			if (Score == requiredScore) {
				Score = 0;
				FinishMinigameRpc();
				AudioManager.Instance.PlayOneShot(AudioEvents.Instance.minigameComplete, transform.position);
				return;
			}

			DoMinigame();
		}

		public IInteractable PickupOrDropObject(bool pickUp,
			Vector3 placePosition)
		{
			OwnerPlayer = null;
			Interacted = false;
			if (tool) tool.SetActive(false);
			instructions.enabled = false;
			return null;
		}

		public virtual IInteractable SecondaryInteract(OwnerPlayer interactor)
		{
			OwnerPlayer = interactor;
			OwnerPlayer.SetCanMove(false);
			OwnerPlayer.SetCooking(true, ShowCursor);
			OwnerPlayer.SetCameraLocation(cameraLocation);
			Interacted = true;
			if (tool) tool.SetActive(true);
			instructions.enabled = true;
			return this;
		}

		public abstract string GetInteractText();

		public bool IsInteractedWith()
		{
			return Interacted;
		}

		[Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
		private void FinishMinigameRpc()
		{
			CompleteMinigame();
			RemoveFood();
		}

		protected abstract bool CheckForIngredients();

		protected virtual void RemoveFood()
		{
		}

		protected abstract void CompleteMinigame();

		protected virtual void DoMinigame()
		{
			if (!MainCamera) return;
			MousePos = Mouse.current.position.ReadValue();
			DidHit = Physics.Raycast(MainCamera.ScreenPointToRay(MousePos), out RayHit) &&
			         RayHit.collider.gameObject && Interacted;
		}

		private void MoveTool()
		{
			if (!tool) return;
			tool.transform.position = RayHit.point + Vector3.up * 0.05f;
		}
	}
}
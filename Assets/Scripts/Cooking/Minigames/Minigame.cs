using System.Collections.Generic;
using Cooking.Minigames.Helpers;
using JetBrains.Annotations;
using Managers;
using MyInterfaces;
using PlayerScripts;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using World;

namespace Cooking.Minigames
{
    [RequireComponent(typeof(Helper))]
    public abstract class Minigame : NetworkBehaviour, IInteractable {
        
        [Header("Minigame Properties")]
        [SerializeField] protected int requiredScore = 10;
        protected int Score;
        protected Vector2 MousePos;
        protected RaycastHit RayHit;
        protected bool DidHit;
        protected List<MoveableObject> CurrentFood = new();
        
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI instructions;
        [SerializeField] [CanBeNull] private GameObject tool;
        public Transform cameraLocation;
        public Transform foodPlaceholder;
        protected Camera MainCamera;
        protected Helper Helper;
        protected OwnerPlayer OwnerPlayer;
        protected bool Interacted;

        protected virtual void Awake()
        {
            MainCamera = Camera.main;
            Helper = GetComponent<Helper>();
        }

        protected virtual void Update()
        {
            MoveTool();
            if (!CheckForIngredients()) return;
            
            if (Score == requiredScore)
            {
                FinishMinigameRpc();
                AudioManager.Instance.PlayOneShot(AudioEvents.Instance.minigameComplete, transform.position);
                Score = 0;
                return;
            }
            DoMinigame();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
        private void FinishMinigameRpc()
        {
            CompleteMinigame();
            RemoveFood();
        }

        protected abstract bool CheckForIngredients();

        protected virtual void RemoveFood() { }

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
            if (tool)
            {
                tool.transform.position = RayHit.point;
            }
        }

        public IInteractable PickupOrDropObject(bool pickUp,
            Vector3 placePosition)
        {
            OwnerPlayer.SetCameraLocation(null);
            OwnerPlayer = null;
            Interacted = false;
            if(tool) tool.SetActive(false);
            instructions.enabled = false;
            return null;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor){
            OwnerPlayer =  interactor;
            OwnerPlayer.SetCanMove(false);
            OwnerPlayer.SetCooking(true);
            OwnerPlayer.SetCameraLocation(cameraLocation);
            Interacted = true;
            if(tool) tool.SetActive(true);
            instructions.enabled = true;
            
            return this;
        }

        public abstract string GetInteractText();

        public bool IsInteractedWith()
        {
            return Interacted;
        }
    }
}




using System.Collections.Generic;
using Cooking.Minigames.Helpers;
using Managers;
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
            if (!CheckForIngredients()) return;
            
            if (Score == requiredScore)
            {
                FinishMinigameRpc();
                AudioManager.Instance.PlayOneShot(AudioEvents.Instance.minigameComplete, transform.position);
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
        
        protected abstract void RemoveFood();
        
        protected abstract void CompleteMinigame();

        protected virtual void DoMinigame()
        {
            MousePos = Mouse.current.position.ReadValue();
            DidHit = Physics.Raycast(MainCamera.ScreenPointToRay(MousePos), out RayHit) &&
                RayHit.collider.gameObject && Interacted;
        }

        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
        {
            OwnerPlayer = null;
            Interacted = false;
            instructions.enabled = false;
            return null;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor){
            OwnerPlayer =  interactor;
            OwnerPlayer.SetCanMove(false);
            OwnerPlayer.SetCooking(true);
            OwnerPlayer.SetCameraLocation(cameraLocation);
            Interacted = true;
            instructions.enabled = true;
            
            return this;
        }

        public abstract string GetInteractName();

        public bool IsInteractedWith()
        {
            return Interacted;
        }
    }
}




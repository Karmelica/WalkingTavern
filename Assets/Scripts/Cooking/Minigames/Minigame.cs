using System.Collections.Generic;
using System.Linq;
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
    //[ExecuteInEditMode]
    [RequireComponent(typeof(Helper))]
    public abstract class Minigame : MonoBehaviour, IInteractable {
        
        public Transform cameraLocation;
        [SerializeField] protected Transform foodPlaceholder;
        [SerializeField] protected int requiredScore = 10;
        protected int Score;
        private bool _isInteractedWith;
        private Camera _mainCamera;
        protected Helper Helper;
        [SerializeField] private TextMeshProUGUI instructions;
        protected List<MoveableObject> CurrentFood = new();

        protected virtual void Start()
        {
            _mainCamera = Camera.main;
            Helper = GetComponent<Helper>();
            Helper.spawnLocation = foodPlaceholder;
        }

        protected virtual void Update()
        {
            EditorUpdate();
            if (!CheckForIngredients()) return;
            Vector3 mousePos = Mouse.current.position.ReadValue();
            
            if (Physics.Raycast(_mainCamera.ScreenPointToRay(mousePos), out RaycastHit hit) &&
                hit.collider.gameObject && _isInteractedWith)
            {
                DoMinigame(hit, mousePos);
            }
            
            if (Score == requiredScore)
            {
                CompleteMinigame();
                RemoveFood();
                AudioManager.Instance.PlayOneShot(AudioEvents.Instance.minigameComplete, transform.position);
            }
        }

        protected abstract bool CheckForIngredients();
        
        protected abstract void RemoveFood();
        protected abstract void CompleteMinigame();
        
        protected abstract void DoMinigame(RaycastHit hit, Vector3 mousePos);

        public IInteractable PrimaryInteract(NetworkBehaviourReference interactor, bool beingPickedUp = true)
        {
            _isInteractedWith = false;
            instructions.enabled = false;
            return null;
        }

        public IInteractable SecondaryInteract(NetworkBehaviourReference interactor)
        {
            if (interactor.TryGet(out OwnerPlayer player))
            {
                player.SetCanMove(false);
                player.SetCooking(true);
                player.SetCameraLocation(cameraLocation);
                _isInteractedWith = true;
                instructions.enabled = true;
            }
            return this;
        }

        public abstract string GetInteractName();

        public bool IsInteractedWith()
        {
            return _isInteractedWith;
        }

#if UNITY_EDITOR
        private void EditorUpdate()
        {
            cameraLocation.LookAt(foodPlaceholder);
        }
        
        private void OnDrawGizmos()
        {
            if (foodPlaceholder)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(foodPlaceholder.position, 0.1f);
            }
            if (cameraLocation)
            {
                Gizmos.color = Color.red;
                Gizmos.matrix = cameraLocation.localToWorldMatrix;
                Gizmos.DrawFrustum(cameraLocation.position, 60, 0.3f, 60, 16 / 9f);
            }
        }
#endif
    }
}




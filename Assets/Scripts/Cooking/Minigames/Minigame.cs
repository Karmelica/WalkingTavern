using System;
using System.Linq;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using World;

namespace Cooking.Minigames
{
    [ExecuteInEditMode]
    public class Minigame : MonoBehaviour, IInteractable
    {
        public Transform cameraLocation;
        [SerializeField] private Transform foodPlaceholder;
        private bool _isInteractedWith;
        
        [SerializeField] private IngredientType[] applicableFood;
        [SerializeField] protected int requiredScore = 10;

        private MinigameHelper helper;
        
        private Camera _mainCamera;
        protected FoodItem CurrentFood;
        protected int Score;

        protected virtual void Start()
        {
            _mainCamera = Camera.main;
            helper = GetComponent<MinigameHelper>();
        }

        protected virtual void Update()
        {
            EditorUpdate();

            Vector3 mousePos = Mouse.current.position.ReadValue();

            if (Physics.Raycast(_mainCamera.ScreenPointToRay(mousePos), out RaycastHit hit) &&
                hit.collider.gameObject && CurrentFood && _isInteractedWith)
            {
                DoMinigame(hit, mousePos);
            }

            if (Score == requiredScore)
            {
                Score = 0;
                Debug.Log("Completed");
                helper.CompleteMinigame(CurrentFood);
            }
        }
        
#if UNITY_EDITOR
        private void EditorUpdate()
        {
            cameraLocation.LookAt(foodPlaceholder);
        }
#endif

        private void OnTriggerEnter(Collider other)
        {
            if (CurrentFood) return;
            if (!other.gameObject.TryGetComponent(out FoodItem foodItem)) return;
            if (applicableFood.Any(applicableFoodItem => applicableFoodItem == foodItem.ingredientType))
            {
                CurrentFood = foodItem;
                CurrentFood.transform.position = foodPlaceholder.position;
                if(NetworkManager.Singleton.IsServer)
                    CurrentFood.PlaceOnMinigameRpc();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (CurrentFood == other.gameObject.GetComponent<FoodItem>())
            {
                CurrentFood = null;
                Score = 0;
            }
        }

        protected virtual void DoMinigame(RaycastHit hit, Vector3 mousePos)
        {
        }

        public IInteractable PrimaryInteract(NetworkBehaviourReference interactor, bool beingPickedUp = true)
        {
            _isInteractedWith = false;
            return null;
        }

        public IInteractable SecondaryInteract(NetworkBehaviourReference interactor)
        {
            if(interactor.TryGet(out OwnerPlayer player))
            {
                player.SetCanMove(false);
                player.SetCooking(true);
                player.SetCameraLocation(cameraLocation);
                _isInteractedWith = true;
            }

            return this;
        }

        public string GetInteractName()
        {
            return "Minigame";
        }

        public bool IsInteractedWith()
        {
            return _isInteractedWith;
        }

        private void OnDrawGizmos()
        {
            if (!cameraLocation || !foodPlaceholder) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(foodPlaceholder.position, 0.1f);
            
            Gizmos.color = Color.red;
            Gizmos.matrix = cameraLocation.localToWorldMatrix;
            Gizmos.DrawFrustum(cameraLocation.position, 60, 0.3f, 60, 16 / 9f);
        }
    }
}




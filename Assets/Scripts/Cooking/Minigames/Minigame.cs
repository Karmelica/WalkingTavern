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
        [SerializeField] private IngredientType[] applicableFood;
        public Transform cameraLocation;
        private Camera _mainCamera;

        protected FoodItem CurrentFood;
        [SerializeField] private Transform foodPlaceholder;

        protected virtual void Start()
        {
            _mainCamera = Camera.main;
        }
        
        protected virtual void Update()
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            
            if (Physics.Raycast(_mainCamera.ScreenPointToRay(mousePos), out RaycastHit hit) && hit.collider.gameObject && CurrentFood)
            {
                DoMinigame(hit, mousePos);
            }

            EditorUpdate();
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
                    CurrentFood.PlaceOnMinigameRpc(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (CurrentFood == other.gameObject.GetComponent<FoodItem>())
            {
                CurrentFood = null;
            }
        }

        protected virtual void DoMinigame(RaycastHit hit, Vector3 mousePos)
        {
        }

        public void PrimaryInteract(NetworkBehaviourReference interactor, bool beingPickedUp = true)
        {
        }

        public void SecondaryInteract(NetworkBehaviourReference interactor)
        {
            if(interactor.TryGet(out OwnerPlayer player))
            {
                player.SetCanMove(false);
                player.SetCooking(true);
                player.SetCameraLocation(cameraLocation);
            }
        }

        public string GetInteractName()
        {
            return "Minigame";
        }

        public bool IsInteractedWith()
        {
            return false;
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

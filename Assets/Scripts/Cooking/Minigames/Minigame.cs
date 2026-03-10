using System;
using System.Linq;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using World;

namespace Cooking.Minigames
{
    public class Minigame : MonoBehaviour, IInteractable
    {
        [SerializeField] private IngredientType[] applicableFood;
        public Transform cameraLocation;

        protected FoodItem CurrentFood;

        protected virtual void Update()
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            
            if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out RaycastHit hit) && hit.collider.gameObject && CurrentFood)
            {
                DoMinigame(hit, mousePos);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CurrentFood) return;
            if (!other.gameObject.TryGetComponent(out FoodItem foodItem)) return;
            if (applicableFood.Any(applicableFoodItem => applicableFoodItem == foodItem.ingredientType))
            {
                CurrentFood = foodItem;
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

        public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
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
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(cameraLocation.transform.position, cameraLocation.transform.position + cameraLocation.transform.forward);
            Gizmos.DrawWireSphere(cameraLocation.transform.position + cameraLocation.transform.forward, 0.1f);
        }
    }
}

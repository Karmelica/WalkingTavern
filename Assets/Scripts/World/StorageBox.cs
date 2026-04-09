using System;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace World
{
    public class StorageBox : NetworkBehaviour, IInteractable
    {
        [SerializeField] private IngredientType ingredientBox;
        [SerializeField] private Image foodIcon;

        private void Start()
        {
            foodIcon.material = new Material(Resources.Load<Material>("Icons/Food/FoodIcon"))
            {
                mainTexture = Resources.Load<Texture>("Icons/Food/" + ingredientBox)
            };
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if(other.TryGetComponent(out FoodItem foodItem) && !foodItem.IsInteractedWith() && foodItem.ingredientType == ingredientBox)
            {
                FoodStorage.Instance.ReturnIngredient(ingredientBox);
                foodItem.NetworkObject.Despawn();
            }
        }

        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
        {
            return null;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor)
        {
            if (FoodStorage.Instance.GetIngredient(ingredientBox))
            {
                SpawnIngredientServerRpc(ingredientBox);
            }
            
            return null;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SpawnIngredientServerRpc(IngredientType ingredientType)
        {
            var ingredient = Instantiate(Resources.Load<GameObject>("Prefabs/Food/Ingredients/" + ingredientType), transform.position + transform.forward, Quaternion.identity);
            ingredient.GetComponent<NetworkObject>().Spawn();
        }

        public string GetInteractName()
        {
            return "\nStorage ("+ ingredientBox +")";
        }

        public bool IsInteractedWith()
        {
            return false;
        }
    }
}

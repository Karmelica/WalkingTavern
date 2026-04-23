using System;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace World.Caravan
{
    public class StorageBox : NetworkBehaviour, IInteractable
    {
        [SerializeField] private IngredientType ingredientBox;
        [SerializeField] private Image foodIcon;
        [SerializeField] private bool unlimited;
        private NetworkVariable<int> _quantity = new();

        private void Awake()
        {
            foodIcon.material = new Material(Resources.Load<Material>("Icons/Food/FoodIcon"))
            {
                mainTexture = Resources.Load<Texture>("Icons/Food/" + ingredientBox)
            };
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            base.OnNetworkSpawn();
            _quantity.Value = FoodStorage.Instance.GetIngredientCount(ingredientBox, unlimited);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if(other.TryGetComponent(out FoodItem foodItem) && !foodItem.IsInteractedWith() && foodItem.ingredientType == ingredientBox)
            {
                FoodStorage.Instance.ReturnIngredient(ingredientBox, unlimited);
                foodItem.NetworkObject.Despawn();
                _quantity.Value = FoodStorage.Instance.GetIngredientCount(ingredientBox, unlimited);
            }
        }

        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
        {
            return null;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor)
        {
            SpawnIngredientServerRpc(ingredientBox);
            return null;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SpawnIngredientServerRpc(IngredientType ingredientType)
        {
            if (!FoodStorage.Instance.GetIngredient(ingredientType, unlimited)) return;
            var ingredient = Instantiate(Resources.Load<GameObject>("Prefabs/Food/Ingredients/" + ingredientType), transform.position + transform.forward, Quaternion.identity);
            ingredient.GetComponent<NetworkObject>().Spawn();
            _quantity.Value = FoodStorage.Instance.GetIngredientCount(ingredientBox, unlimited);
        }

        public string GetInteractText()
        {
            if(unlimited) return $"\nStorage ({ingredientBox})";
            return $"\nStorage ({ingredientBox}: {_quantity.Value})";
        }

        public bool IsInteractedWith()
        {
            return false;
        }
    }
}

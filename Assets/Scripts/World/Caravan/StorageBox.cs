using System;
using MyInterfaces;
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
            base.OnNetworkSpawn();
            if (!IsServer) return;
            FoodStorage.Instance.ReturnIngredient(ingredientBox, unlimited);
            FoodStorage.Instance.ReturnIngredient(ingredientBox, unlimited);
            FoodStorage.Instance.ReturnIngredient(ingredientBox, unlimited);
            FoodStorage.Instance.ReturnIngredient(ingredientBox, unlimited);
            FoodStorage.Instance.ReturnIngredient(ingredientBox, unlimited);
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

        public IInteractable PickupOrDropObject(bool pickUp,
            Vector3 placePosition)
        {
            return null;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor)
        {
            CheckIngredientServerRpc();
            return null;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void CheckIngredientServerRpc(RpcParams rpcParams = default)
        {
            if (!FoodStorage.Instance.GetIngredient(ingredientBox, unlimited)) return;
            _quantity.Value = FoodStorage.Instance.GetIngredientCount(ingredientBox, unlimited);
            var i = Instantiate(Resources.Load<GameObject>("Prefabs/Food/Ingredients/" + ingredientBox), transform.position + transform.forward * 0.5f, Quaternion.identity);
            i.GetComponent<NetworkObject>().Spawn(true);
            i.GetComponent<MoveableObject>().PlayPickupSound();
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

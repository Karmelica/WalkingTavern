using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Managers
{
    [RequireComponent(typeof(Collider))]
    public class PickupableIngredient : NetworkBehaviour, IInteractable
    {
        [SerializeField] private IngredientType ingredientType;
        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
        {
            PickupThisIngredientServerRpc();
            return null;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void PickupThisIngredientServerRpc()
        {
            NetworkObject.Despawn();
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor)
        {
            return null;
        }

        public string GetInteractText()
        {
            return ingredientType.ToString();
        }

        public bool IsInteractedWith()
        {
            return false;
        }
    }
}

using Unity.Netcode;
using UnityEngine;

namespace World
{
    public class DishItem : MoveableObject
    {
        public DishType dishType;
        public bool canBeCooked;
        private NetworkVariable<float> _cookingProgress = new();

        private void OnValidate()
        {
            gameObject.name = dishType.ToString();
        }
        
        public void Despawn()
        {
            DespawnItemServerRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void DespawnItemServerRpc()
        {
            NetworkObject.Despawn();
        }
        
        [Rpc(SendTo.Server)]
        public void CookRpc()
        {
            if(canBeCooked)
                _cookingProgress.Value += Time.deltaTime;
        }
        
        public override string GetInteractText()
        {
            return $"Pick up {dishType.ToString()}";
        }

    }
}

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
            if (!IsOwner) return;
            DespawnItemServerRpc();
        }

        [Rpc(SendTo.Server)]
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
        
        public override string GetInteractName()
        {
            return dishType.ToString();
        }

    }
}

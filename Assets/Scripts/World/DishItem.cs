using Unity.Netcode;
using UnityEngine;

namespace World
{
    public class DishItem : MoveableObject
    {
        public DishType dishType;

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
        
        public override string GetInteractName()
        {
            return dishType.ToString();
        }
    }
}

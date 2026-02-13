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
        protected override void SetTransformsServerRpc(NetworkBehaviourReference interactor, bool pickingUp = true)
        {
            base.SetTransformsServerRpc(interactor, pickingUp);
            //colli.excludeLayers = pickingUp ? LayerMask.NameToLayer("Customer") : 0;
        }

        [Rpc(SendTo.Server)]
        private void DespawnItemServerRpc()
        {
            NetworkObject.Despawn();
        }
    }
}

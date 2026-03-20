using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public class DishHelper : Helper
    {
        [SerializeField] private DishType dishPrefab;
        
        public override void DespawnObject(MoveableObject objectToDespawn)
        {
            if (objectToDespawn.TryGetComponent(out ProcessedFoodItem processedFoodItem))
            {
                DespawnObjectRpc(processedFoodItem.NetworkObject);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public override void SpawnObjectRpc(string path = null)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Food/Dishes/" + dishPrefab);
            var dish = Instantiate(prefab, spawnLocation.position + Vector3.up, Quaternion.identity);
            dish.GetComponent<NetworkObject>().Spawn();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public override void DespawnObjectRpc(NetworkObjectReference objectReference)
        {
            if(objectReference.TryGet(out var networkObject))
            {
                networkObject.Despawn();
            }
        }
    }
}

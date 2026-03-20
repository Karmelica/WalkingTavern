using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public class IngredientHelper : Helper
    {
        public override void DespawnObject(MoveableObject objectToDespawn)
        {
            if(objectToDespawn.TryGetComponent(out FoodItem foodItem))
            {
                var type = (ProcessedIngredientType)foodItem.ingredientType;
                DespawnObjectRpc(foodItem.NetworkObject);
                
                SpawnObjectRpc("Prefabs/Food/ProcessedIngredients/" + type);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public override void SpawnObjectRpc(string path = null)
        {
            var prefab = Resources.Load<GameObject>(path);
            var ingredient = Instantiate(prefab, spawnLocation.position, Quaternion.identity);
            ingredient.GetComponent<NetworkObject>().Spawn();
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

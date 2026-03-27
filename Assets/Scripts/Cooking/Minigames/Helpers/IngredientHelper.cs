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
                var types = foodItem.ingredientProducts;
                if (!IsServer) return;
                foodItem.NetworkObject.Despawn();

                foreach (var type in types)
                {
                    SpawnObject(type);
                }
            }
        }

        public override void SpawnObject(GameObject prefab = null)
        {
            var ingredient = Instantiate(prefab, spawnLocation.position + Vector3.up, Quaternion.identity);
            ingredient.GetComponent<NetworkObject>().Spawn();
        }
    }
}

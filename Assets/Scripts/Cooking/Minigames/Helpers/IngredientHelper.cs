using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public class IngredientHelper : Helper
    {
        public override void DespawnObject<T>(T objectToDespawn)
        {
            if (!IsServer) return;
            if(objectToDespawn is FoodItem foodItem)
            {
                var products = foodItem.ingredientProducts;
                foreach (var product in products) SpawnObject(product);
                foodItem.NetworkObject.Despawn();
            }
        }

        protected override void SpawnObject(GameObject prefab)
        {
            base.SpawnObject(prefab);
            var ingredient = Instantiate(prefab, spawnLocation.position + Vector3.up, Quaternion.identity);
            ingredient.GetComponent<NetworkObject>().Spawn();
        }
    }
}

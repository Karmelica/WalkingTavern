using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public class IngredientHelper : Helper
    {
        public override void DespawnObject(MoveableObject objectToDespawn)
        {
            if (!IsServer) return;
            if (!objectToDespawn.TryGetComponent(out FoodItem foodItem)) return;
            var products = foodItem.ingredientProducts;
            foodItem.NetworkObject.Despawn();

            foreach (var product in products) SpawnObject(product);
        }

        protected virtual void SpawnObject(GameObject prefab)
        {
            var ingredient = Instantiate(prefab, spawnLocation.position + Vector3.up, Quaternion.identity);
            ingredient.GetComponent<NetworkObject>().Spawn();
        }

        public override void SpawnObject(DishType dishType)
        {
            //unused
        }
    }
}

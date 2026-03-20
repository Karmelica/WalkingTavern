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
            if(objectToDespawn is FoodItem foodItem)
            {
                var type = (ProcessedIngredientType)foodItem.ingredientType;
                foodItem.NetworkObject.Despawn();
                
                SpawnObject("Prefabs/Food/ProcessedIngredients/" + type);
            }
        }

        public override void SpawnObject(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            var ingredient = Instantiate(prefab, spawnLocation.position, Quaternion.identity);
            ingredient.GetComponent<NetworkObject>().Spawn();
        }
    }
}

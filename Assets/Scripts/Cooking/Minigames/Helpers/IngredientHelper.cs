using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public class IngredientHelper : Helper
    {
        public override void CompleteMinigame(List<MoveableObject> objectsToChange)
        {
            if(objectsToChange[0] is FoodItem)
            {
                var foodItem = objectsToChange[0] as FoodItem;
                var type = (ProcessedIngredientType)foodItem.ingredientType;

                foodItem.NetworkObject.Despawn();

                var prefab = Resources.Load<GameObject>("Prefabs/Food/ProcessedIngredients/" + type);
                var ingredient = Instantiate(prefab, spawnLocation.position, Quaternion.identity);
                ingredient.GetComponent<NetworkObject>().Spawn();
            }
        }

    }
}

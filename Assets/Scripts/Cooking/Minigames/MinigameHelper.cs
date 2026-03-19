using System;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames
{
    public class MinigameHelper : NetworkBehaviour
    {
        public void Awake()
        {
            if (!IsServer)
            {
                enabled = false;
            }
        }

        public void CompleteMinigame(FoodItem currentFood)
        {
            var location = currentFood.transform.position;
            var type = (ProcessedIngredientType)currentFood.ingredientType;
            
            currentFood.NetworkObject.Despawn();
            
            var prefab = Resources.Load<GameObject>("Prefabs/Food/ProcessedIngredients/" + type);
            var ingredient = Instantiate(prefab, location, Quaternion.identity);
            ingredient.GetComponent<NetworkObject>().Spawn();
        }
    }
}

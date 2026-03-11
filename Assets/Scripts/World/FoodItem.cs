using System;
using UnityEngine;

namespace World
{
    public class FoodItem : MoveableObject
    {
        public IngredientType ingredientType;

        public void CompleteMinigame()
        {
            if (IsServer)
            {
                NetworkObject.Despawn();
            }
        }
    }
}
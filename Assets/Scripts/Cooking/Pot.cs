using UnityEngine;
using World;

namespace Cooking
{
    public class Pot : DishMakingPlace
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out ProcessedFoodItem foodItem)) return;
            TryAddIngredient(foodItem);
        }
        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out ProcessedFoodItem foodItem)) return;
            TryRemoveIngredient(foodItem);
        }
    }
}

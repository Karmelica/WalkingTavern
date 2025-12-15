using System.Collections.Generic;
using UnityEngine;
using World;

namespace Cooking
{
    public class CookingPlace : MonoBehaviour
    {
        private readonly Dictionary<IngredientType, int> _placedIngredients = new();
        private readonly Dictionary<IngredientType, int> _requiredIngredients = new()
        {
            { IngredientType.Cheese, 2 },
            { IngredientType.Lettuce, 1 }
        };
        [SerializeField] private SkillCheck skillCheck;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<FoodItem>(out var foodItem)) return;
            if (!_placedIngredients.TryAdd(foodItem.ingredientType, 1))
            {
                _placedIngredients[foodItem.ingredientType]++;
            }

            //Debug.Log($"Ingredient {foodItem.ingredientType} count: {_placedIngredients[foodItem.ingredientType]}");
            CheckForIngredients();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<FoodItem>(out var foodItem)) return;
            if (!_placedIngredients.TryGetValue(foodItem.ingredientType, out var ingredient)) return;
            if (ingredient <= 0) return;
            _placedIngredients[foodItem.ingredientType]--;
                
            //Debug.Log($"Ingredient {foodItem.ingredientType} count: {_placedIngredients[foodItem.ingredientType]}");
            CheckForIngredients();
        }
        
        private void CheckForIngredients()
        {
            foreach (var requiredIngredient in _requiredIngredients)
            {
                var requiredIngredientKey = requiredIngredient.Key;
                var requiredCount = requiredIngredient.Value;
                if (_placedIngredients.TryGetValue(requiredIngredientKey, out var placedCount))
                {
                    Debug.Log($"Placed {placedCount}/{requiredCount} of {requiredIngredientKey}");
                }
                else
                {
                    Debug.Log($"Placed 0/{requiredCount} of {requiredIngredientKey}");
                }
            }
            //Debug.Log("All ingredients placed! Starting skill check.");
            //skillCheck.gameObject.SetActive(true);
        }
    }
}

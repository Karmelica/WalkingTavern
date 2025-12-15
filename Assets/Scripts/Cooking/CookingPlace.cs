using System;
using System.Collections.Generic;
using TMPro;
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
        
        [SerializeField] private SkillCheckObject skillCheck;
        [SerializeField] private TextMeshProUGUI ingredientListText;

        private void Start()
        {
            CheckForIngredients();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<FoodItem>(out var foodItem)) return;
            if (!_placedIngredients.TryAdd(foodItem.ingredientType, 1))
            {
                _placedIngredients[foodItem.ingredientType]++;
            }
            
            CheckForIngredients();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<FoodItem>(out var foodItem)) return;
            if (!_placedIngredients.TryGetValue(foodItem.ingredientType, out var ingredient)) return;
            if (ingredient <= 0)
            {
                _placedIngredients.Remove(foodItem.ingredientType);
                return;
            }
            _placedIngredients[foodItem.ingredientType]--;
                
            CheckForIngredients();
        }
        
        private void CheckForIngredients()
        {
            var ingredientsList = "Ingredients:\n";
            foreach (var (requiredIngredientKey, requiredCount) in _requiredIngredients)
            {
                if (_placedIngredients.TryGetValue(requiredIngredientKey, out var placedCount))
                {
                    ingredientsList += $"{placedCount}/{requiredCount} of {requiredIngredientKey}\n";
                }
                else
                {
                    ingredientsList += $"0/{requiredCount} of {requiredIngredientKey}\n";
                }
            }
            ingredientListText.text = ingredientsList;
        }
    }
}

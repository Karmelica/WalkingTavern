using System;
using System.Collections.Generic;
using Cooking.ScriptableObjects;
using TMPro;
using UnityEngine;
using World;

namespace Cooking
{
    
    public class CookingPlace : MonoBehaviour
    {
        private readonly Dictionary<IngredientType, int> _placedIngredients = new();
        [SerializeField] private Recipe recipe;
        
        [SerializeField] private SkillCheckObject skillCheck;
        [SerializeField] private TextMeshProUGUI ingredientListText;

        private void Start()
        {
            foreach (var ingredientType in Enum.GetValues(typeof(IngredientType)))
            {
                _placedIngredients.TryAdd((IngredientType)ingredientType, 0);
            }
            UpdateRecipeText();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<FoodItem>(out var foodItem)) return;
            if (!_placedIngredients.TryAdd(foodItem.ingredientType, 1))
            {
                _placedIngredients[foodItem.ingredientType]++;
            }
            
            UpdateRecipeText();
            if(IsRecipeComplete())
            {
                skillCheck.enabled = true;
            }
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
                
            UpdateRecipeText();
        }
        
        private void UpdateRecipeText()
        {
            ingredientListText.text = "Ingredients:\n";
            foreach (var ingredient in recipe.ingredients)
            {
                var ingredientType = ingredient.ingredient;
                var ingredientQuantity = ingredient.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) continue;
                ingredientListText.text += $"{placedCount}/{ingredientQuantity} of {ingredientType}\n";
            }
        }
        
        private bool IsRecipeComplete()
        {
            foreach (var ingredient in recipe.ingredients)
            {
                var ingredientType = ingredient.ingredient;
                var ingredientQuantity = ingredient.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) return false;
                
                if (placedCount < ingredientQuantity)
                {
                    return false;
                }
            }
            return true;

            //recipe complete
            //skillCheck.gameObject.SetActive(true);
        }
    }
}

using System;
using System.Collections.Generic;
using Cooking.ScriptableObjects;
using TMPro;
using UnityEngine;
using World;

namespace Cooking.Minigames
{
    public abstract class DishMinigame : Minigame
    {
        protected Dictionary<ProcessedIngredientType, int> _placedIngredients = new();
        [SerializeField] protected Recipe recipe;
        
        [SerializeField] private TextMeshProUGUI ingredientListText;

        protected override void Start()
        {
            base.Start();
            foreach (var processedIngredientType in Enum.GetValues(typeof(ProcessedIngredientType)))
            {
                _placedIngredients.TryAdd((ProcessedIngredientType)processedIngredientType, 0);
            }
            UpdateRecipeText();
            
            if (!IsServer) return;
            Helper.SpawnSomeIngredients();
        }

        protected void TryAddIngredient(ProcessedFoodItem foodItem)
        {
            _placedIngredients[foodItem.ingredientType]++;
            if (!CurrentFood.Contains(foodItem))
            {
                CurrentFood.Add(foodItem);
            }
            UpdateRecipeText();
        }

        protected void TryRemoveIngredient(ProcessedFoodItem foodItem)
        {
            if (!_placedIngredients.TryGetValue(foodItem.ingredientType, out _)) return;
            if (CurrentFood.Contains(foodItem))
            { 
                CurrentFood.Remove(foodItem);
            }
            _placedIngredients[foodItem.ingredientType]--;
                
            UpdateRecipeText();
        }

        private void CompleteRecipe()
        {
            foreach (var ingredient in recipe.ingredients)
            {
                var ingredientType = ingredient.ingredientType;
                var quantity = ingredient.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) continue;
                if (placedCount < quantity) continue;

                int removedCount = 0;
                for (int i = CurrentFood.Count - 1; i >= 0 && removedCount < quantity; i--)
                {
                    var item = CurrentFood[i];
                    if (item.TryGetComponent(out ProcessedFoodItem foodItemComponent) && foodItemComponent.ingredientType == ingredientType)
                    {
                        CurrentFood.Remove(item);
                        Helper.DespawnObject(item);
                        removedCount++;
                    }
                }
                _placedIngredients[ingredientType] -= removedCount;
            }
            Helper.SpawnObject();
            
            UpdateRecipeText();
        }

        protected override void CompleteMinigame()
        {
            CompleteRecipe();
        }

        protected override void RemoveFood()
        {
            Score = 0;
        }

        private void UpdateRecipeText()
        {
            ingredientListText.text = "Ingredients:\n";
            foreach (var ingredient in recipe.ingredients)
            {
                var ingredientType = ingredient.ingredientType;
                var ingredientQuantity = ingredient.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) continue;
                ingredientListText.text += $"{ingredientType} {ingredientQuantity}/{placedCount}\n";
            }
        }
        
        protected override bool CheckForIngredients()
        {
            foreach (var ingredient in recipe.ingredients)
            {
                var ingredientType = ingredient.ingredientType;
                var ingredientQuantity = ingredient.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) return false;
                
                if (placedCount < ingredientQuantity)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

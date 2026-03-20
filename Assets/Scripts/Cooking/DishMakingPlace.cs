using System;
using System.Collections.Generic;
using Cooking.ScriptableObjects;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking
{
    public class DishMakingPlace : NetworkBehaviour
    {
        private readonly Dictionary<ProcessedIngredientType, int> _placedIngredients = new();
        private readonly List<ProcessedFoodItem> _placedFoodItems = new();
        [SerializeField] private Recipe recipe;
        
        [SerializeField] private TextMeshProUGUI ingredientListText;

        private void Start()
        {
            foreach (var processedIngredientType in Enum.GetValues(typeof(ProcessedIngredientType)))
            {
                _placedIngredients.TryAdd((ProcessedIngredientType)processedIngredientType, 0);
            }
            UpdateRecipeText();
            
            if (IsServer)
                SpawnSomeIngredients();
            
        }

        private void SpawnSomeIngredients()
        {
            var ingredientTypes = Enum.GetValues(typeof(IngredientType));
            foreach (var type in ingredientTypes)
            {
                for(var i = 0; i < 5; i++){
                    var prefab = Resources.Load<GameObject>("Prefabs/Food/Ingredients/" + type);
                    var position = transform.position + new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 3f,
                        UnityEngine.Random.Range(0f, 8f));
                    var ingredient = Instantiate(prefab, position, Quaternion.identity);
                    ingredient.GetComponent<NetworkObject>().Spawn();
                }
            }
        }

        protected void TryAddIngredient(ProcessedFoodItem foodItem)
        {
            _placedIngredients[foodItem.ingredientType]++;
            if (!_placedFoodItems.Contains(foodItem))
            {
                _placedFoodItems.Add(foodItem);
            }
            UpdateRecipeText();
            
            if (!IsRecipeComplete()) return;
            
            //enable skillcheck here
            CompleteRecipe();
        }

        protected void TryRemoveIngredient(ProcessedFoodItem foodItem)
        {
            if (!_placedIngredients.TryGetValue(foodItem.ingredientType, out _)) return;
            if (_placedFoodItems.Contains(foodItem))
            { 
                _placedFoodItems.Remove(foodItem);
            }
            _placedIngredients[foodItem.ingredientType]--;
                
            UpdateRecipeText();
        }

        private void CompleteRecipe()
        {
            foreach (var foodItem in recipe.ingredients)
            {
                var ingredientType = foodItem.ingredient;
                var quantity = foodItem.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) continue;
                if (placedCount < quantity) continue;

                int removedCount = 0;
                for (int i = _placedFoodItems.Count - 1; i >= 0 && removedCount < quantity; i--)
                {
                    var item = _placedFoodItems[i];
                    if (item.TryGetComponent(out ProcessedFoodItem foodItemComponent) && foodItemComponent.ingredientType == ingredientType)
                    {
                        item.gameObject.SetActive(false);
                        _placedFoodItems.RemoveAt(i);
                        removedCount++;
                    }
                }
                _placedIngredients[ingredientType] -= removedCount;
            }
            
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
                ingredientListText.text += $"{ingredientType} {ingredientQuantity}/{placedCount}\n";
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
        }
    }
}

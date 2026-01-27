using System;
using System.Collections.Generic;
using Cooking.ScriptableObjects;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking
{
    public class CookingPlace : NetworkBehaviour
    {
        private readonly Dictionary<IngredientType, int> _placedIngredients = new();
        [SerializeField] private List<GameObject> _placedFoodItems = new();
        [SerializeField] private Recipe recipe;
        
        [SerializeField] private SkillCheckObject skillCheck;
        [SerializeField] private TextMeshProUGUI ingredientListText;
        [SerializeField] private Collider triggerCollider;

        private void Start()
        {
            foreach (var ingredientType in Enum.GetValues(typeof(IngredientType)))
            {
                _placedIngredients.TryAdd((IngredientType)ingredientType, 0);
            }
            UpdateRecipeText();
        }
        
        public void ChangeRecipe(Recipe newRecipe)
        {
            recipe = newRecipe;
            UpdateRecipeText();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<FoodItem>(out var foodItem)) return;
            if (!_placedIngredients.TryAdd(foodItem.ingredientType, 1))
            {
                _placedIngredients[foodItem.ingredientType]++;
                if (!_placedFoodItems.Contains(other.gameObject))
                {
                    _placedFoodItems.Add(other.gameObject);
                }
            }
            UpdateRecipeText();
            
            if (!IsRecipeComplete()) return;
            CompleteRecipe();
            skillCheck.enabled = true;
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<FoodItem>(out var foodItem)) return;
            if (!_placedIngredients.TryGetValue(foodItem.ingredientType, out var ingredient)) return;
            if (_placedFoodItems.Contains(other.gameObject))
            { 
                _placedFoodItems.Remove(other.gameObject);
            }
            if (ingredient <= 0)
            {
                _placedIngredients.Remove(foodItem.ingredientType);
                return;
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
                    if (item.TryGetComponent<FoodItem>(out var foodItemComponent) && foodItemComponent.ingredientType == ingredientType)
                    {
                        Destroy(item);
                        _placedFoodItems.RemoveAt(i);
                        removedCount++;
                    }
                }
                _placedIngredients[ingredientType] -= removedCount;
            }
            
            if(IsServer){
                var prefab = Resources.Load<GameObject>("Prefabs/Food/Dishes/" + recipe.dishType);
                var dish = Instantiate(prefab, transform.position + Vector3.up * 1.0f, Quaternion.identity);
                dish.GetComponent<NetworkObject>().Spawn();
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

using System;
using System.Collections.Generic;
using Cooking.ScriptableObjects;
using NaughtyAttributes;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using World;

namespace Cooking.Minigames
{
    public abstract class DishMinigame : Minigame
    {
        [Header("Minigame Properties")]
        [SerializeField] protected NetworkVariable<DishType> dishType = new();
        [Expandable]
        protected Recipe Recipe;
        private readonly Dictionary<ProcessedIngredientType, int> _placedIngredients = new();
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI ingredientListText;
        
        protected override void Awake()
        {
            base.Awake();
            foreach (var processedIngredientType in Enum.GetValues(typeof(ProcessedIngredientType)))
            {
                _placedIngredients.TryAdd((ProcessedIngredientType)processedIngredientType, 0);
            }

            Recipe = GetItems.GetRecipeByDishType(dishType.Value);
            UpdateRecipeText();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            dishType.OnValueChanged += DishChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            dishType.OnValueChanged -= DishChanged;
        }

        public void DishTypeChanged(DishType type)
        {
            ChangeDishTypeRpc(type);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ChangeDishTypeRpc(DishType type)
        {
            dishType.Value = type;
        }

        private void DishChanged(DishType previousValue, DishType newValue)
        {
            Recipe = GetItems.GetRecipeByDishType(newValue);
            UpdateRecipeText();
        }

        protected void TryAddIngredient(ProcessedFoodItem foodItem)
        {
            _placedIngredients[foodItem.processedIngredientType]++;
            if (!CurrentFood.Contains(foodItem))
            {
                CurrentFood.Add(foodItem);
                foodItem.isOnMinigame = true;
            }
            UpdateRecipeText();
        }

        protected void TryRemoveIngredient(ProcessedFoodItem foodItem)
        {
            if (!_placedIngredients.TryGetValue(foodItem.processedIngredientType, out _)) return;
            if (CurrentFood.Contains(foodItem))
            { 
                CurrentFood.Remove(foodItem);
                foodItem.isOnMinigame = false;
            }
            _placedIngredients[foodItem.processedIngredientType]--;
                
            UpdateRecipeText();
        }

        private void CompleteRecipe()
        {
            foreach (var ingredient in Recipe.ingredients)
            {
                var ingredientType = ingredient.ingredientType;
                var quantity = ingredient.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) continue;
                if (placedCount < quantity) continue;

                int removedCount = 0;
                for (int i = CurrentFood.Count - 1; i >= 0 && removedCount < quantity; i--)
                {
                    var item = CurrentFood[i];
                    if (item.TryGetComponent(out ProcessedFoodItem foodItemComponent) && foodItemComponent.processedIngredientType == ingredientType)
                    {
                        CurrentFood.Remove(item);
                        Helper.DespawnObject(item);
                        removedCount++;
                    }
                }
                _placedIngredients[ingredientType] -= removedCount;
            }
            Helper.SpawnObject(dishType.Value);
            
            UpdateRecipeText();
        }

        protected override void CompleteMinigame()
        {
            CompleteRecipe();
        }

        private void UpdateRecipeText()
        {
            ingredientListText.text = "Ingredients:";
            foreach (var ingredient in Recipe.ingredients)
            {
                var ingredientType = ingredient.ingredientType;
                var ingredientQuantity = ingredient.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) continue;
                ingredientListText.text += $"\n{ingredientQuantity}/{placedCount} {ingredientType}   ";
            }
        }
        
        protected override bool CheckForIngredients()
        {
            foreach (var ingredient in Recipe.ingredients)
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

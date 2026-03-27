using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using World;
using Debug = UnityEngine.Debug;

namespace Cooking.Minigames
{
    public class StackingMinigame : DishMinigame
    {
        public Plane plane;
        private ProcessedFoodItem lastInteractedObject;
        private List<ProcessedFoodItem> recipeQueue;

        protected override void Start()
        {
            base.Start();
            plane = new Plane(foodPlaceholder.forward * -1, foodPlaceholder.position);
        }

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

        protected override void DoMinigame()
        {
            var distance = 0f;
            MousePos = Mouse.current.position.ReadValue();
            var mouseRay = MainCamera.ScreenPointToRay(MousePos);
            
            DidHit = Interacted && plane.Raycast(mouseRay, out distance);
            if (!DidHit) return;

            if (OwnerPlayer.IsHoldingLMB() && lastInteractedObject) {
                lastInteractedObject.MoveOnMinigame(mouseRay.GetPoint(distance)); 
            }
            if(OwnerPlayer.IsHoldingLMB() && !lastInteractedObject) {
                if (Physics.Raycast(MainCamera.ScreenPointToRay(MousePos), out RayHit, float.PositiveInfinity, 1<<7, QueryTriggerInteraction.Ignore) &&
                    RayHit.collider.gameObject) {
                    if (RayHit.collider.gameObject.TryGetComponent(out ProcessedFoodItem foodItem)) {
                        lastInteractedObject = foodItem;
                    }
                }
            }

            if (!OwnerPlayer.IsHoldingLMB()) {
                if(lastInteractedObject)
                {
                    lastInteractedObject.PlaceDownRpc();
                    lastInteractedObject = null;
                }

                if (CheckForRecipe())
                {
                    Score = requiredScore;
                }
            }

        }

        private bool CheckForRecipe()
        {
            Debug.ClearDeveloperConsole();
            recipeQueue = new();
            //tylko jeśli wszystkie składniki są na miejscu
            foreach (var ingredient in recipe.ingredients)
            {
                ProcessedFoodItem food = null;
                var quantity = 0;
                foreach (var moveableObject in CurrentFood) {
                    if (moveableObject is ProcessedFoodItem foodItem &&
                        foodItem.ingredientType == ingredient.ingredientType)
                    {
                        food = foodItem;
                        quantity++;
                    }
                }

                if (quantity >= ingredient.quantity) {
                    recipeQueue.Add(food);
                }
                if(quantity < ingredient.quantity) {
                    recipeQueue.Clear();
                    return false;
                }
            }

            ProcessedFoodItem lastFoodItem = null;
            string message = "";

            message += "RecipeQueue: \n";
            foreach (var foodItem in recipeQueue)
            {
                message += foodItem.ingredientType + "\n";
            }

            message += "\n";

            foreach (var foodItem in recipeQueue) {
                if (!lastFoodItem) {
                    lastFoodItem = foodItem;
                    continue;
                }
                
                message += "Comparing " + foodItem.ingredientType + " to " + lastFoodItem.ingredientType + "\n";
                message += foodItem.transform.localPosition.y + " " + lastFoodItem.transform.localPosition.y + "\n";
                
                if (foodItem.transform.localPosition.y > lastFoodItem.transform.localPosition.y) {
                    message += "! " + foodItem.ingredientType + " is higher than " + lastFoodItem.ingredientType + "\n";
                    Debug.Log(message);
                    return false;
                }
                lastFoodItem = foodItem;
            }

            message += "All good";
            Debug.Log(message);
            return true;
        }

        public override string GetInteractName()
        {
            return "Stacking Minigame";
        }
    }
}

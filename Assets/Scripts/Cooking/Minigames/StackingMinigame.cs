using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using World;

namespace Cooking.Minigames
{
	public class StackingMinigame : DishMinigame
	{
		private ProcessedFoodItem lastInteractedObject;
		public Plane plane;
		private List<ProcessedFoodItem> recipeQueue;

		protected override void Awake()
		{
			base.Awake();
			plane = new Plane(foodPlaceholder.forward * -1, foodPlaceholder.position);
		}

		private void OnTriggerEnter(Collider other)
		{
			AddCollidedFood(other);
		}

		private void OnTriggerExit(Collider other)
		{
			RemoveCollidedFood(other);
		}

		private void AddCollidedFood(Collider other)
		{
			if (!other.TryGetComponent(out ProcessedFoodItem foodItem)) return;
			foodItem.OnObjectDisable += RemoveCollidedFood;
			TryAddIngredient(foodItem);
		}

		private void RemoveCollidedFood(Collider other)
		{
			if (!other.TryGetComponent(out ProcessedFoodItem foodItem)) return;
			foodItem.OnObjectDisable -= RemoveCollidedFood;
			TryRemoveIngredient(foodItem);
		}

		protected override void DoMinigame()
		{
			var distance = 0f;
			MousePos = Mouse.current.position.ReadValue();
			var mouseRay = MainCamera.ScreenPointToRay(MousePos);

			DidHit = Interacted && plane.Raycast(mouseRay, out distance);
			if (!DidHit) return;

			if (OwnerPlayer.IsHoldingLmb()) {
				if (lastInteractedObject) {
					lastInteractedObject.MoveOnMinigame(mouseRay.GetPoint(distance));
				} else if (Physics.Raycast(MainCamera.ScreenPointToRay(MousePos), out RayHit, float.PositiveInfinity, 1 << 7, QueryTriggerInteraction.Ignore) && RayHit.collider.gameObject && RayHit.collider.gameObject.TryGetComponent(out ProcessedFoodItem foodItem)) {
					lastInteractedObject = foodItem;
				}
			} else {
				if (lastInteractedObject) {
					lastInteractedObject.PlaceDown();
					lastInteractedObject = null;
				}
				if (CheckForRecipe()) Score = requiredScore;
			}
		}

		private bool CheckForRecipe()
		{
			recipeQueue = new List<ProcessedFoodItem>();
			//tylko jeśli wszystkie składniki są na miejscu
			foreach (var ingredient in Recipe.ingredients) {
				ProcessedFoodItem food = null;
				var quantity = 0;
				foreach (var moveableObject in CurrentFood)
					if (moveableObject is ProcessedFoodItem foodItem &&
					    foodItem.processedIngredientType == ingredient.ingredientType) {
						food = foodItem;
						quantity++;
					}

				if (quantity >= ingredient.quantity) recipeQueue.Add(food);
				if (quantity < ingredient.quantity) {
					recipeQueue.Clear();
					return false;
				}
			}

			ProcessedFoodItem lastFoodItem = null;

			foreach (var foodItem in recipeQueue) {
				if (!lastFoodItem) {
					lastFoodItem = foodItem;
					continue;
				}

				if (foodItem.transform.position.y > lastFoodItem.transform.position.y) return false;
				lastFoodItem = foodItem;
			}

			return true;
		}

		public override string GetInteractText()
		{
			return "Stacking";
		}
	}
}
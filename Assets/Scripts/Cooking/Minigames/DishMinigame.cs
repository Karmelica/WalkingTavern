using System;
using System.Collections.Generic;
using System.Linq;
using Cooking.ScriptableObjects;
using Managers;
using NaughtyAttributes;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames
{
	public abstract class DishMinigame : Minigame
	{
		[Header("Minigame Properties")] [SerializeField]
		protected NetworkVariable<DishType> dishType = new();

		[Header("Components")] [SerializeField]
		private TextMeshProUGUI ingredientListText;

		private readonly Dictionary<ProcessedIngredientType, int> _placedIngredients = new();
		protected bool AlreadySpawned = false;

		[Expandable] protected Recipe Recipe;

		protected override void Awake()
		{
			base.Awake();
			foreach (var processedIngredientType in Enum.GetValues(typeof(ProcessedIngredientType)))
				_placedIngredients.TryAdd((ProcessedIngredientType)processedIngredientType, 0);

			Recipe = GetItems.GetRecipeByDishType(dishType.Value);
			UpdateRecipeText();
		}

		protected override void Update()
		{
			if (CurrentFood.Any()) {
				foreach (var food in CurrentFood.Where(food => !food.gameObject.activeInHierarchy)) {
					if (!food.TryGetComponent(out ProcessedFoodItem item)) break;
					CurrentFood.Remove(food);
					TryRemoveIngredient(item);
					break;
				}
			}

			base.Update();
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			dishType.OnValueChanged += OnDishValueChanged;
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			dishType.OnValueChanged -= OnDishValueChanged;
		}

		private void OnDishValueChanged(DishType previousValue, DishType newValue)
		{
			Recipe = GetItems.GetRecipeByDishType(newValue);
			UpdateRecipeText();
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

		protected void TryAddIngredient(ProcessedFoodItem foodItem)
		{
			_placedIngredients[foodItem.processedIngredientType]++;
			if (!CurrentFood.Contains(foodItem)) {
				CurrentFood.Add(foodItem);
				foodItem.isOnMinigame = true;
			}

			UpdateRecipeText();
		}

		protected void TryRemoveIngredient(ProcessedFoodItem foodItem)
		{
			if (!_placedIngredients.TryGetValue(foodItem.processedIngredientType, out _)) return;
			if (CurrentFood.Contains(foodItem)) {
				CurrentFood.Remove(foodItem);
				foodItem.isOnMinigame = false;
			}

			_placedIngredients[foodItem.processedIngredientType]--;

			UpdateRecipeText();
		}

		private void CompleteRecipe()
		{
			foreach (var ingredient in Recipe.ingredients) {
				var ingredientType = ingredient.ingredientType;
				var quantity = ingredient.quantity;

				if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) continue;
				if (placedCount < quantity) continue;

				var removedCount = 0;
				for (var i = CurrentFood.Count - 1; i >= 0 && removedCount < quantity; i--) {
					var item = CurrentFood[i];
					if (item.TryGetComponent(out ProcessedFoodItem foodItemComponent) &&
					    foodItemComponent.processedIngredientType == ingredientType) {
						Helper.DespawnObject(item);
						removedCount++;
					}
				}
			}

			Helper.SpawnObject(dishType.Value);
			AudioManager.Instance.StopStirring();

			UpdateRecipeText();
			AlreadySpawned = false;
		}

		protected override void CompleteMinigame()
		{
			CompleteRecipe();
		}

		private void UpdateRecipeText()
		{
			ingredientListText.text = "Ingredients:";
			foreach (var ingredient in Recipe.ingredients) {
				var ingredientType = ingredient.ingredientType;
				var ingredientQuantity = ingredient.quantity;

				if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) continue;
				ingredientListText.text +=
					$"\n{placedCount}/{ingredientQuantity} {ingredientType.ToString().SplitBigLetter().ReplaceWordWith("Processed","Sliced")}   ";
			}
		}

		protected override bool CheckForIngredients()
		{
			foreach (var ingredient in Recipe.ingredients) {
				var ingredientType = ingredient.ingredientType;
				var ingredientQuantity = ingredient.quantity;

				if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) return false;

				if (placedCount < ingredientQuantity) return false;
			}

			return true;
		}
	}
}
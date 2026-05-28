using System.Linq;
using UnityEngine;
using World;

namespace Cooking.Minigames
{
	public abstract class IngredientMinigame : Minigame
	{
		[SerializeField] private IngredientType[] applicableFood;

		protected override void Awake()
		{
			ShowCursor = false;
			base.Awake();
		}

		private void OnTriggerEnter(Collider other)
		{
			AddFood(other);
		}

		private void OnTriggerExit(Collider other)
		{
			RemoveCollidedFood(other);
		}

		private void AddFood(Collider other)
		{
			if (CurrentFood.Any()) return;
			if (!other.gameObject.TryGetComponent(out FoodItem foodItem)) return;
			if (applicableFood.Any(ingredientType => ingredientType == foodItem.ingredientType)) {
				foodItem.OnObjectDisable += RemoveCollidedFood;
				CurrentFood.Add(foodItem);
				CurrentFood[0].transform.position = new Vector3(foodPlaceholder.position.x,
					CurrentFood[0].transform.position.y, foodPlaceholder.position.z);
				CurrentFood[0].isOnMinigame = true;
			}
		}

		private void RemoveCollidedFood(Collider other)
		{
			if (!CheckForIngredients()) return;
			var food = CurrentFood[0];
			if (food == other.gameObject.GetComponent<FoodItem>()) {
				food.OnObjectDisable -= RemoveCollidedFood;
				CurrentFood[0].isOnMinigame = false;
				RemoveFood();
			}
		}

		protected override bool CheckForIngredients()
		{
			return CurrentFood.Any();
		}

		protected override void CompleteMinigame()
		{
			Helper.DespawnObject(CurrentFood[0]);
		}

		protected override void RemoveFood()
		{
			CurrentFood.Clear();
		}
	}
}
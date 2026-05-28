using System.Collections.Generic;
using UnityEngine;

namespace World.Caravan
{
	public class FoodStorage : MonoBehaviour
	{
		public static FoodStorage Instance;
		private readonly Dictionary<IngredientType, int> _gatheredIngredients = new();

		private void Awake()
		{
			if (Instance != null) {
				Destroy(gameObject);
			} else {
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
		}

		public bool GetIngredient(IngredientType requestedIngredient, bool isUnlimited)
		{
			if (isUnlimited) return true;
			if (_gatheredIngredients.TryGetValue(requestedIngredient, out var value)) {
				if (value > 0) {
					_gatheredIngredients[requestedIngredient]--;
					return true;
				}
			}

			return false;
		}

		public void ReturnIngredient(IngredientType returnedIngredient, bool isUnlimited)
		{
			if (isUnlimited) return;
			if (!_gatheredIngredients.TryAdd(returnedIngredient, 1)) _gatheredIngredients[returnedIngredient]++;
		}

		public int GetIngredientCount(IngredientType requestedIngredient, bool isUnlimited)
		{
			return isUnlimited ? 0 : _gatheredIngredients.GetValueOrDefault(requestedIngredient, 0);
		}
	}
}
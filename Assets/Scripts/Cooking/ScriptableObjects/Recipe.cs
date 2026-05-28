using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Cooking.ScriptableObjects
{
	[CreateAssetMenu(fileName = "New Recipe", menuName = "Cooking/Recipe")]
	public class Recipe : ScriptableObject
	{
		public DishType dishType;
		public List<Ingredients> ingredients;
		[MinMaxSlider(0f, 300f)] public Vector2 cookingMinMax;
	}

	[Serializable]
	public class Ingredients
	{
		public ProcessedIngredientType ingredientType;
		public int quantity;
	}
}
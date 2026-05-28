using System.Collections.Generic;
using Cooking.ScriptableObjects;
using UnityEngine;
using World;

namespace Cooking
{
	public static class GetItems
	{
		private static readonly List<Recipe> Recipes = new();
		private static readonly Dictionary<DishType, Recipe> RecipesDict = new();
		private static readonly Dictionary<uint, MoveableObject> ObjectsDict = new();

		[RuntimeInitializeOnLoadMethod]
		private static void Initialize()
		{
			Recipes.AddRange(Resources.LoadAll<Recipe>("ScriptableObjects/Cooking"));
			foreach (var recipe in Recipes) RecipesDict.TryAdd(recipe.dishType, recipe);

			var objects = Resources.LoadAll<MoveableObject>("Prefabs/Food");
			foreach (var obj in objects) ObjectsDict.TryAdd(obj.ID, obj);
		}

		public static Recipe GetRecipeByDishType(DishType dishType)
		{
			return RecipesDict[dishType];
		}

		public static MoveableObject GetObjectByID(uint id)
		{
			return ObjectsDict[id];
		}
	}
}
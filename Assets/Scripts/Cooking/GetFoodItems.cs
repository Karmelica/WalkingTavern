using System.Collections.Generic;
using Cooking.ScriptableObjects;
using UnityEngine;

namespace Cooking
{
    public static class GetFoodItems
    {
        private static readonly List<Recipe> Recipes = new();
        private static readonly Dictionary<DishType, Recipe> RecipesDict = new ();
        
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Recipes.AddRange(Resources.LoadAll<Recipe>("ScriptableObjects/Cooking"));
            foreach (var recipe in Recipes)
            {
                RecipesDict.TryAdd(recipe.dishType, recipe);
            }
        } 
        
        public static Recipe GetRecipeByDishType(DishType dishType)
        {
            return RecipesDict[dishType];
        }
    }
}
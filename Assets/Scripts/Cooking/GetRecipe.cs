using System.Collections.Generic;
using Cooking.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Cooking
{
    public static class GetRecipe
    {

        private static List<Recipe> _recipes = new();
        private static Dictionary<DishType, Recipe> _recipesDict = new ();

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _recipes.AddRange(Resources.LoadAll<Recipe>("ScriptableObjects/Cooking"));
            foreach (var recipe in _recipes)
            {
                _recipesDict.TryAdd(recipe.dishType, recipe);
            }
        } 
        
        public static Recipe GetRecipeByDishType(DishType dishType)
        {
            return _recipesDict[dishType];
        }
        
        public static List<Recipe> GetRecipes()
        {
            return _recipes;
        }
    }
}
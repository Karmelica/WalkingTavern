using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.ScriptableObjects
{
        [CreateAssetMenu(fileName = "New Recipe", menuName = "Cooking/Recipe")]
        public class Recipe : ScriptableObject
        {
                public DishType dishType;
                public List<Ingredients> ingredients;
        }
        
        [Serializable]
        public class Ingredients
        {
                public ProcessedIngredientType ingredientType;
                public int quantity;
        }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.ScriptableObjects
{
        [CreateAssetMenu(fileName = "New Recipe", menuName = "Cooking/Recipe")]
        public class Recipe : ScriptableObject
        {
                public string recipeName;
                public List<Ingredients> ingredients;
        }
        
        [Serializable]
        public class Ingredients
        {
                public IngredientType ingredient;
                public int quantity;
        }
}

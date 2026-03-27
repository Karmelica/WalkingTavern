using UnityEngine;

namespace World
{
    public class FoodItem : MoveableObject
    {
        public IngredientType ingredientType;
        public GameObject[] ingredientProducts;

        private void OnValidate()
        {
            gameObject.name = ingredientType.ToString();
        }
        
        public override string GetInteractName()
        {
            return ingredientType.ToString();
        }
    }
}
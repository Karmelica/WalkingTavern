using System;

namespace World
{
    public class ProcessedFoodItem : MoveableObject
    {
        public ProcessedIngredientType ingredientType;

        private void OnValidate()
        {
            gameObject.name = ingredientType.ToString();
        }

        public override string GetInteractText()
        {
            return ingredientType.ToString();
        }

    }
}

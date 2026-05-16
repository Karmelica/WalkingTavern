using System;

namespace World
{
    public class ProcessedFoodItem : MoveableObject
    {
        public ProcessedIngredientType processedIngredientType;

        private void OnValidate()
        {
            gameObject.name = processedIngredientType.ToString();
        }

        public override string GetInteractText()
        {
            var str = processedIngredientType.ToString();
            return $"Pick up {Utilis.SplitBigLetter(str)}";
        }

    }
}

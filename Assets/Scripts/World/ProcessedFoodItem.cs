namespace World
{
    public class ProcessedFoodItem : MoveableObject
    {
        public ProcessedIngredientType ingredientType;
        
        public override string GetInteractName()
        {
            return ingredientType.ToString();
        }
    }
}

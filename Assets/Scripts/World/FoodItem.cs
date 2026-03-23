namespace World
{
    public class FoodItem : MoveableObject
    {
        public IngredientType ingredientType;

        public override string GetInteractName()
        {
            return ingredientType.ToString();
        }
    }
}
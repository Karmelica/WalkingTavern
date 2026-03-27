using Unity.Behavior;

public enum IngredientType
{
    Lettuce,
    Raspberry,
    Cheese,
    Meat,
    Toast,
    Baguette
}

public enum ProcessedIngredientType
{
    ProcessedLettuce,
    ProcessedRaspberry,
    ProcessedCheese,
    ProcessedMeat,
    ProcessedToastTop,
    ProcessedToastBottom,
    ProcessedBaguetteTop,
    ProcessedBaguetteBottom
}

[BlackboardEnum]
public enum DishType
{
    Hamburger,
    LettuceSoup,
    CheeseCake,
    Casserole
}

[BlackboardEnum]
public enum CustomerState
{
    Ordering,
    WaitingForFood,
    Eating,
    Leaving
}
using Unity.Behavior;

public enum IngredientType
{
    Lettuce,
    Raspberry,
    Cheese,
    Meat,
    Toast,
    Baguette,
    Mushroom
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
    ProcessedBaguetteBottom,
    ProcessedMushroom
}

[BlackboardEnum]
public enum DishType
{
    Hamburger,
    LettuceSoup,
    CheeseCake,
    Casserole,
    MushroomSoup
}

[BlackboardEnum]
public enum CustomerState
{
    Ordering,
    WaitingForFood,
    Eating,
    Leaving
}
using Unity.Behavior;

public enum IngredientType
{
    Lettuce,
    Raspberry,
    Cheese,
    Toast,
    Baguette,
    Mushroom,
    Karambola
}

public enum ProcessedIngredientType
{
    ProcessedLettuce,
    ProcessedRaspberry,
    ProcessedCheese,
    ProcessedToast,
    ProcessedBaguetteTop,
    ProcessedBaguetteBottom,
    ProcessedMushroom,
    ProcessedKarambola
}

[BlackboardEnum]
public enum DishType
{
    Casserole,
    MushroomSoup,
    ToastCheese,
    ToastKarambola,
    FruitSoup,
    AppleTart
}

[BlackboardEnum]
public enum CustomerState
{
    Ordering,
    WaitingForFood,
    Eating,
    Leaving
}
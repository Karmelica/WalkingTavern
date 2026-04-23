using Unity.Behavior;

public enum IngredientType
{
    MushroomBlue,
    MushroomRed,
    MushroomWhite,
    Raspberry,
    Grape,
    Apple,
    Lettuce,
    Cheese,
    Toast,
    Baguette,
    Karambola,
}

public enum ProcessedIngredientType
{
    ProcessedMushroom,
    ProcessedRaspberry,
    ProcessedGrape,
    ProcessedApple,
    ProcessedLettuce,
    ProcessedCheese,
    ProcessedToast,
    ProcessedBaguetteBottom,
    ProcessedBaguetteTop,
    ProcessedKarambola,
}

[BlackboardEnum]
public enum DishType
{
    DishCasserole,
    DishMushroomSoup,
    DishToastCheese,
    DishToastKarambola,
    DishFruitSoup,
    DishAppleTart
}

[BlackboardEnum]
public enum CustomerState
{
    Ordering,
    WaitingForFood,
    Eating,
    Leaving
}
using Unity.Behavior;

public enum IngredientType
{
    MushroomBlue,
    MushroomRed,
    MushroomWhite,
    Raspberry,
    Grape,
    Apple,
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
    DishToastKarambola,
    DishToastJam,
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
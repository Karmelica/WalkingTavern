using Unity.Behavior;
using UnityEngine;

public enum SkillCheckResult
{
    Completed,
    Fail,
    Success,
    Perfect
}

public enum IngredientType
{
    Lettuce,
    Tomato,
    Cheese,
    Meat,
    Bun
}

public enum ProcessedIngredientType
{
    ProcessedLettuce,
    ProcessedTomato,
    ProcessedCheese,
    ProcessedMeat,
    ProcessedBun
}

[BlackboardEnum]
public enum DishType
{
    Hamburger,
    LettuceSoup,
    CheeseCake,
    Sandwich
}

[BlackboardEnum]
public enum CustomerState
{
    Ordering,
    WaitingForFood,
    Eating,
    Leaving
}
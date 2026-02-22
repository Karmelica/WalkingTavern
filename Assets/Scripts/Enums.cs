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

[BlackboardEnum]
public enum DishType
{
    Hamburger,
    LettuceSoup,
    CheeseCake
}

[BlackboardEnum]
public enum CustomerState
{
    Ordering,
    WaitingForFood,
    Eating,
    Leaving
}
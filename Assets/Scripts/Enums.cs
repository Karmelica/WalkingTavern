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

public enum DishType
{
    Hamburger,
    LettuceSoup,
    CheeseCake
}

public enum CustomerState
{
    WaitingInLine,
    Ordering,
    WaitingForFood,
    Eating,
    Leaving
}
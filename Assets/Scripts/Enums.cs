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
    Bun,
    None
}

public enum CustomerState
{
    WaitingInLine,
    Ordering,
    WaitingForFood,
    Eating,
    Leaving
}
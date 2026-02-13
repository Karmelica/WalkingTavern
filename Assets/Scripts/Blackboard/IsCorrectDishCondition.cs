using Cooking.ScriptableObjects;
using System;
using Unity.Behavior;
using UnityEngine;
using World;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Correct Dish", story: "Is [Dish] requested [Recipe]", category: "Conditions", id: "497b66987e521a3398730f4448274e0d")]
public partial class IsCorrectDishCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Dish;
    [SerializeReference] public BlackboardVariable<Recipe> Recipe;

    public override bool IsTrue()
    {
        if (Dish.Value.TryGetComponent<DishItem>(out var dishItem))
        {
            return dishItem.dishType == Recipe.Value.dishType;
        }
        return false;
    }
}

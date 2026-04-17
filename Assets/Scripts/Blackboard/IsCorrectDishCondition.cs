using Cooking.ScriptableObjects;
using System;
using Managers;
using Unity.Behavior;
using UnityEngine;
using World;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Correct Dish", story: "Is [Dish] [Self] requested dish", category: "Conditions", id: "497b66987e521a3398730f4448274e0d")]
public partial class IsCorrectDishCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Dish;
    [SerializeReference] public BlackboardVariable<Customer> Self;

    public override bool IsTrue()
    {
        if (Dish.Value.TryGetComponent<DishItem>(out var dishItem))
        {
            if(dishItem.dishType == Self.Value.requestedDish.Value){
                AIManager.OnScoreChanged?.Invoke(100);
                Self.Value.RemoveDish();
                return true;
            }
        }
        AIManager.OnScoreChanged?.Invoke(-50);
        Self.Value.RemoveDish();
        return false;
    }
}

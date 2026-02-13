using System;
using Unity.Behavior;
using UnityEngine;
using World;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Interacted With", story: "Is [Dish] held", category: "Conditions", id: "8b91f9686dda684b943a65227f0b2bf8")]
public partial class IsInteractedWithCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Dish;

    public override bool IsTrue()
    {
        if (Dish.Value.TryGetComponent<MoveableObject>(out var moveableObject))
        {
            if (moveableObject.IsInteractedWith()) return true;
        }
        return false;
    }
}

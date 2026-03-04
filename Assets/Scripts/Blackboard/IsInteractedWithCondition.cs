using System;
using Unity.Behavior;
using UnityEngine;
using World;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Interacted With", story: "[Dish] [Is] held", category: "Conditions", id: "8b91f9686dda684b943a65227f0b2bf8")]
public partial class IsInteractedWithCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Dish;
    [SerializeReference] public BlackboardVariable<bool> Is = new(true);

    public override bool IsTrue()
    {
        if (Dish.Value.TryGetComponent<MoveableObject>(out var moveableObject))
        {
            if (moveableObject.IsInteractedWith() == Is) return true;
        }
        return false;
    }
}

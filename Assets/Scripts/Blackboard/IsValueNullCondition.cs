using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Value Null", story: "Is [Value] null", category: "Conditions", id: "92a647aa9252b8376681c57c0d9f689f")]
public partial class IsValueNullCondition : Condition
{
    [SerializeReference] public BlackboardVariable<MonoBehaviour> Value;

    public override bool IsTrue()
    {
        return Value.Value == null;
    }
}

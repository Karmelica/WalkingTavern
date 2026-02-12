using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Look Forward", story: "[Self] look at front of the [Object]", category: "Action", id: "586a71383554764a01407bec2a92231a")]
public partial class LookForwardAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Object;


    protected override Status OnStart()
    {
        base.OnStart();
        Look();
        return Status.Success;
    }

    private void Look()
    {
        Self.Value.transform.LookAt(Object.Value.position + Object.Value.forward);
    }
}


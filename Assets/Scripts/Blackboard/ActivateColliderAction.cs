using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Activate Collider", story: "Set [Collider] to [Active]", category: "Action", id: "03b86c158aa2cc15e7492e96e7831d65")]
public partial class ActivateColliderAction : Action
{
    [SerializeReference] public BlackboardVariable<Collider> Collider;
    [SerializeReference] public BlackboardVariable<bool> Active;

    protected override Status OnStart()
    {
        Collider.Value.enabled = Active.Value;
        return Status.Success;
    }
}


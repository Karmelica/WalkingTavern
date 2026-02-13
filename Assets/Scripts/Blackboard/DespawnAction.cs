using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Despawn", story: "Despawn [SelfCustomer]", category: "Action", id: "3ff50cd2bdd1f4811b7db4009f418e1a")]
public partial class DespawnAction : Action
{
    [SerializeReference] public BlackboardVariable<Customer> SelfCustomer;

    protected override Status OnStart()
    {
        SelfCustomer.Value.DespawnCustomer();
        return Status.Success;
    }
}


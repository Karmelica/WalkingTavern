using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "StartTimer", story: "[Customer] starts timer", category: "Action", id: "4b6afb22878e5ed4953001bec52b790e")]
public partial class StartTimerAction : Action
{
    [SerializeReference] public BlackboardVariable<Customer> Customer;

    protected override Status OnStart()
    {
        Customer.Value.StartTimerRpc();
        return Status.Success;
    }
}


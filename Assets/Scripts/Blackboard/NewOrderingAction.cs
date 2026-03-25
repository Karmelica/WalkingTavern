using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NewOrdering", story: "Assign new [OrderingCustomer] from [CustomersInLine]", category: "Action", id: "af8f2270ea4c95444cf2f3ee70d8120b")]
public partial class NewOrderingAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> OrderingCustomer;
    [SerializeReference] public BlackboardVariable<List<GameObject>> CustomersInLine;

    protected override Status OnStart()
    {
        OrderingCustomer.Value = CustomersInLine.Value[0];
        return Status.Success;
    }
}


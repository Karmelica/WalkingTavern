using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Remove From List", story: "Remove [Customer] from [CustomerList]", category: "Action", id: "d631e1a9cce9278329dcc79f735551c5")]
public partial class RemoveFromListAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Customer;
    [SerializeReference] public BlackboardVariable<List<GameObject>> CustomerList;

    protected override Status OnStart()
    {
        if (CustomerList.Value.Contains(Customer))
        {
            CustomerList.Value.Remove(Customer);
            return Status.Success;
        }
        return Status.Failure;
    }
}


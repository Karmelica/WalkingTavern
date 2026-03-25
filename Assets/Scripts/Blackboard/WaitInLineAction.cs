using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Wait In Line", story: "[Self] waits in line", category: "Action", id: "2bdeb3e76f0baa557b164b7b3b20f45e")]
public partial class WaitInLineAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> LinePlace;
    //[SerializeReference] public BlackboardVariable<GameObject> CustomerInFront;
    [SerializeReference] public BlackboardVariable<List<GameObject>> CustomersInLine;

    protected override Status OnStart()
    {
        if (CustomersInLine.Value.Contains(Self))
        {
            var index = CustomersInLine.Value.IndexOf(Self);
            var orderingCustomer = CustomersInLine.Value[0];
            var customerTransform = orderingCustomer.transform;
            LinePlace.Value = customerTransform.position - customerTransform.forward * index * 1.5f;

            return Status.Success;
        }

        return Status.Failure;
    }
}


using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Wait In Line", story: "[Self] sets CustomerInFront", category: "Action", id: "2bdeb3e76f0baa557b164b7b3b20f45e")]
public partial class WaitInLineAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> CustomerInFrontLocation;
    [SerializeReference] public BlackboardVariable<GameObject> CustomerInFront;
    [SerializeReference] public BlackboardVariable<List<GameObject>> CustomersInLine;

    protected override Status OnStart()
    {
        if (CustomersInLine.Value.Contains(Self))
        {
            var index = CustomersInLine.Value.IndexOf(Self);
            if (index == 0)
            {
                var customer = CustomersInLine.Value[^1];
                var customerTransform = customer.transform;
                CustomerInFront.Value = customer;
                CustomerInFrontLocation.Value = customerTransform.position - customerTransform.forward;
            }
            else
            {
                var customer = CustomersInLine.Value[index - 1];
                var customerTransform = customer.transform;
                CustomerInFront.Value = customer;
                CustomerInFrontLocation.Value = customerTransform.position - customerTransform.forward;
            }

            return Status.Success;
        }

        return Status.Failure;
    }
}


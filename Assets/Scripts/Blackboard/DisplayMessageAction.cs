using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Display message", story: "[Customer] displays [message]", category: "Action", id: "57cad856bf51d5f9819b02a38ce741c4")]
public partial class DisplayMessageAction : Action
{
    [SerializeReference] public BlackboardVariable<Customer> Customer;
    [SerializeReference] public BlackboardVariable<string> Message;

    protected override Status OnStart()
    {
        Customer.Value.ShowMessageRpc(Message.Value);
        return Status.Success;
    }
}


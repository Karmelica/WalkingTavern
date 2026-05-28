using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable]
[GeneratePropertyBag]
[NodeDescription("Display message", story: "[Customer] displays [message]", category: "Action",
	id: "57cad856bf51d5f9819b02a38ce741c4")]
public class DisplayMessageAction : Action
{
	[SerializeReference] public BlackboardVariable<Customer> Customer;
	[SerializeReference] public BlackboardVariable<string> Message;

	protected override Status OnStart()
	{
		Customer.Value.ShowMessageRpc(Message.Value);
		return Status.Success;
	}
}
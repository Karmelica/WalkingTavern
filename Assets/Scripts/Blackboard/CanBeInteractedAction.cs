using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable]
[GeneratePropertyBag]
[NodeDescription("CanBeInteracted", story: "Set [Customer] CanInteract to [Bool]", category: "Action",
	id: "12db9c39e1bd0301cb2e2d0499d89841")]
public class CanBeInteractedAction : Action
{
	[SerializeReference] public BlackboardVariable<Customer> Customer;
	[SerializeReference] public BlackboardVariable<bool> Bool;

	protected override Status OnStart()
	{
		Customer.Value.orderTaken.Value = !Bool.Value;
		return Status.Success;
	}
}
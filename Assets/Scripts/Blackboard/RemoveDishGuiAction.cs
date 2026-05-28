using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable]
[GeneratePropertyBag]
[NodeDescription("RemoveDishGUI", story: "[Customer] removes requested dish from list", category: "Action",
	id: "f51a8dddad7e02aa4ec94b2329d16e31")]
public class RemoveDishGuiAction : Action
{
	[SerializeReference] public BlackboardVariable<Customer> Customer;

	protected override Status OnStart()
	{
		Customer.Value.RemoveDish();
		return Status.Success;
	}
}
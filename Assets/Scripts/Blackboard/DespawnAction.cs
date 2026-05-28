using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable]
[GeneratePropertyBag]
[NodeDescription("Despawn", story: "Despawn [SelfCustomer]", category: "Action",
	id: "3ff50cd2bdd1f4811b7db4009f418e1a")]
public class DespawnAction : Action
{
	[SerializeReference] public BlackboardVariable<Customer> SelfCustomer;

	protected override Status OnStart()
	{
		SelfCustomer.Value.DespawnCustomer();
		return Status.Success;
	}
}
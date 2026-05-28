using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable]
[GeneratePropertyBag]
[NodeDescription("Wait In Line", story: "[Self] waits in line", category: "Action",
	id: "2bdeb3e76f0baa557b164b7b3b20f45e")]
public class WaitInLineAction : Action
{
	[SerializeReference] public BlackboardVariable<GameObject> Self;
	[SerializeReference] public BlackboardVariable<Vector3> LinePlace;
	[SerializeReference] public BlackboardVariable<List<GameObject>> CustomersInLine;
	[SerializeReference] public BlackboardVariable<Transform> OrderingLocation;

	protected override Status OnStart()
	{
		if (CustomersInLine.Value.Contains(Self)) {
			var index = CustomersInLine.Value.IndexOf(Self);
			LinePlace.Value = OrderingLocation.Value.position - OrderingLocation.Value.forward * index * 1.5f;

			return Status.Success;
		}

		return Status.Failure;
	}
}
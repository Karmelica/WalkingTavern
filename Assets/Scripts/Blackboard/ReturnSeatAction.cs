using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable]
[GeneratePropertyBag]
[NodeDescription("Return Seat", story: "[Customer] returns [Seat]", category: "Action",
	id: "11a80524a61c4408e52557a366648c47")]
public class ReturnSeatAction : Action
{
	[SerializeReference] public BlackboardVariable<Customer> Customer;
	[SerializeReference] public BlackboardVariable<Transform> Seat;

	protected override Status OnStart()
	{
		if (Customer == null || Seat == null) return Status.Failure;

		Customer.Value.ReturnSeat(Seat);
		return Status.Success;
	}
}
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable]
[GeneratePropertyBag]
[Condition("Try get seat", story: "Has [Customer] assigned [Seat]", category: "Conditions",
	id: "c30a40f807e3635233933a9cfba3bcff")]
public class TryGetSeatCondition : Condition
{
	[SerializeReference] public BlackboardVariable<Customer> Customer;
	[SerializeReference] public BlackboardVariable<Transform> Seat;

	public override void OnStart()
	{
		base.OnStart();
		if (Seat.Value != null) return;
		if (Customer.Value.TryGetSeat(out var seat)) Seat.Value = seat;
	}

	public override bool IsTrue()
	{
		if (Seat.Value) return true;
		return false;
	}
}
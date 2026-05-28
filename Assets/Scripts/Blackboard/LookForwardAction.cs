using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable]
[GeneratePropertyBag]
[NodeDescription("Look Forward", story: "[Self] look at front of the [Object]", category: "Action",
	id: "586a71383554764a01407bec2a92231a")]
public class LookForwardAction : Action
{
	[SerializeReference] public BlackboardVariable<GameObject> Self;
	[SerializeReference] public BlackboardVariable<Transform> Object;


	protected override Status OnStart()
	{
		base.OnStart();
		Look();
		return Status.Success;
	}

	private void Look()
	{
		Self.Value.transform.LookAt(Object.Value.position + Object.Value.forward);
	}
}
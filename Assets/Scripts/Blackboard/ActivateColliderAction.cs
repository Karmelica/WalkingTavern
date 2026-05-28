using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable]
[GeneratePropertyBag]
[NodeDescription("Activate Collider", story: "Set [Collider] to [Active]", category: "Action",
	id: "03b86c158aa2cc15e7492e96e7831d65")]
public class ActivateColliderAction : Action
{
	[SerializeReference] public BlackboardVariable<Collider> Collider;
	[SerializeReference] public BlackboardVariable<bool> Active;

	protected override Status OnStart()
	{
		Collider.Value.enabled = Active.Value;
		return Status.Success;
	}
}
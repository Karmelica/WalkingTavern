using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using World;
using Action = Unity.Behavior.Action;

[Serializable]
[GeneratePropertyBag]
[NodeDescription("Eat Dish", story: "Eat [Dish]", category: "Action", id: "3e6ccd3904528619496a2cafdee7f1c5")]
public class EatDishAction : Action
{
	[SerializeReference] public BlackboardVariable<GameObject> Dish;

	protected override Status OnStart()
	{
		if (Dish.Value.TryGetComponent<DishItem>(out var dish)) {
			dish.Despawn();
			return Status.Success;
		}

		return Status.Failure;
	}
}
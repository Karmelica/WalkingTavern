using System;
using Managers;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using World;

[Serializable]
[GeneratePropertyBag]
[Condition("Is Correct Dish", story: "Is [Dish] [Self] requested dish", category: "Conditions",
	id: "497b66987e521a3398730f4448274e0d")]
public class IsCorrectDishCondition : Condition
{
	[SerializeReference] public BlackboardVariable<GameObject> Dish;
	[SerializeReference] public BlackboardVariable<Customer> Self;

	public override bool IsTrue()
	{
		if (Dish.Value.TryGetComponent<DishItem>(out var dishItem)) {
			if (dishItem.dishType == Self.Value.requestedDish.Value) {
				if (dishItem.IsCookedEnough()) {
					AIManager.OnScoreChanged?.Invoke(100);
					Self.Value.ShowMessageRpc("This is good");
				} else {
					AIManager.OnScoreChanged?.Invoke(20);
					Self.Value.ShowMessageRpc("It's not good enough");
				}

				return true;
			}
		}

		AIManager.OnScoreChanged?.Invoke(-50);
		_ = Self.Value.ShowMessage("This is bad");
		return false;
	}
}
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
	public class IngredientHelper : Helper
	{
		public override void DespawnObject<T>(T objectToDespawn)
		{
			if (!IsServer) return;
			if (objectToDespawn is FoodItem foodItem) {
				var products = foodItem.ingredientProducts;
				var offset = 0f;
				foreach (var product in products) {
					SpawnObject(product, offset);
					offset += 0.2f;
				}
				foodItem.NetworkObject.Despawn();
			}
		}

		protected override void SpawnObject(GameObject prefab, float offset = 0)
		{
			base.SpawnObject(prefab, offset);
			var ingredient = Instantiate(prefab, spawnLocation.position + Vector3.up * offset, Quaternion.identity);
			ingredient.GetComponent<NetworkObject>().Spawn();
		}
	}
}
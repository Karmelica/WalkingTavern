using System;
using Unity.Netcode;
using UnityEngine;
using World;
using Random = UnityEngine.Random;

namespace Cooking.Minigames.Helpers
{
	public abstract class Helper : NetworkBehaviour
	{
		[SerializeField] protected bool shouldSpawnSomeIngredients = true;
		public Transform spawnLocation;
		[SerializeField] protected GameObject particles;

		public override void OnNetworkSpawn()
		{
			if (!IsServer) enabled = false;
			SpawnSomeIngredients();
		}

		public virtual void DespawnObject<T>(T objectToDespawn) where T : MoveableObject
		{
		}

		public virtual void SpawnObject(DishType dishType)
		{
			if (particles) Instantiate(particles, spawnLocation.position, Quaternion.identity);
		}

		protected virtual void SpawnObject(GameObject prefab, float offset = 0)
		{
			if (particles) Instantiate(particles, spawnLocation.position, Quaternion.identity);
		}

		private void SpawnSomeIngredients()
		{
			if (!shouldSpawnSomeIngredients) return;
			var ingredientTypes = Enum.GetValues(typeof(IngredientType));
			foreach (var type in ingredientTypes)
				for (var i = 0; i < 5; i++) {
					var prefab = Resources.Load<GameObject>("Prefabs/Food/Ingredients/" + type);
					var position = transform.position + Random.insideUnitSphere + Vector3.up;
					var ingredient = Instantiate(prefab, position, Quaternion.identity);
					ingredient.GetComponent<NetworkObject>().Spawn();
				}
		}
	}
}
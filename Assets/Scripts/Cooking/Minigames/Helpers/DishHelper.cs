using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public class DishHelper : Helper
    {
        public override void DespawnObject<T>(T objectToDespawn)
        {
            if (!IsServer) return;
            if (objectToDespawn is ProcessedFoodItem processedFoodItem) processedFoodItem.NetworkObject.Despawn();
        }

        public override void SpawnObject(DishType dishType)
        {
            if (!IsServer) return;
            base.SpawnObject(dishType);
            var original = Resources.Load<GameObject>("Prefabs/Food/Dishes/" + dishType);
            var dish = Instantiate(original, spawnLocation.position + Vector3.up, Quaternion.identity);
            dish.GetComponent<NetworkObject>().Spawn();
        }
    }
}

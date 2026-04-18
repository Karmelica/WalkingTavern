using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public class DishHelper : Helper
    {
        public void OnEnable()
        {
            base.OnNetworkSpawn();
        }

        public override void DespawnObject(MoveableObject objectToDespawn)
        {
            if (objectToDespawn.TryGetComponent(out ProcessedFoodItem processedFoodItem))
            {
                if (!IsServer) return;
                processedFoodItem.NetworkObject.Despawn();
            }
        }

        public override void SpawnObject(DishType dishType)
        {
            if (!IsServer) return;
            var original = Resources.Load<GameObject>("Prefabs/Food/Dishes/" + dishType);
            var dish = Instantiate(original, spawnLocation.position + Vector3.up, Quaternion.identity);
            dish.GetComponent<NetworkObject>().Spawn();
        }
    }
}

using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public class DishHelper : Helper
    {
        [SerializeField] private DishType dishPrefab;

        public void OnEnable()
        {
            base.OnNetworkSpawn();
            SpawnSomeIngredients();
        }

        public override void DespawnObject(MoveableObject objectToDespawn)
        {
            if (objectToDespawn.TryGetComponent(out ProcessedFoodItem processedFoodItem))
            {
                if (!IsServer) return;
                processedFoodItem.NetworkObject.Despawn();
            }
        }

        public override void SpawnObject(string path = null)
        {
            if (!IsServer) return;
            var prefab = Resources.Load<GameObject>("Prefabs/Food/Dishes/" + dishPrefab);
            var dish = Instantiate(prefab, spawnLocation.position + Vector3.up, Quaternion.identity);
            dish.GetComponent<NetworkObject>().Spawn();
        }
    }
}

using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public class DishHelper : Helper
    {
        [SerializeField] private DishType dishPrefab;
        
        public override void DespawnObject(MoveableObject objectToDespawn)
        {
            if (objectToDespawn is ProcessedFoodItem processedFoodItem)
            {
                processedFoodItem.NetworkObject.Despawn();
            }
        }

        public override void SpawnObject(string path = null)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Food/Dishes/" + dishPrefab);
            var dish = Instantiate(prefab, spawnLocation.position + Vector3.up, Quaternion.identity);
            dish.GetComponent<NetworkObject>().Spawn();
        }
    }
}

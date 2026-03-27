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

        private void Awake()
        {
            if (!IsServer)
            {
                enabled = false;
            }
        }

        public abstract void DespawnObject(MoveableObject objectToDespawn);
        
        public abstract void SpawnObject(GameObject prefab = null);
        
        public void SpawnSomeIngredients()
        {
            if (!shouldSpawnSomeIngredients) return;
            var ingredientTypes = Enum.GetValues(typeof(IngredientType));
            foreach (var type in ingredientTypes)
            {
                for(var i = 0; i < 5; i++){
                    var prefab = Resources.Load<GameObject>("Prefabs/Food/Ingredients/" + type);
                    var position = transform.position + Random.insideUnitSphere + Vector3.up;
                    var ingredient = Instantiate(prefab, position, Quaternion.identity);
                    ingredient.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
    }
}

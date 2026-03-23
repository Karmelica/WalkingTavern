using System;
using Unity.Netcode;
using UnityEngine;
using World;
using Random = UnityEngine.Random;

namespace Cooking.Minigames.Helpers
{
    public abstract class Helper : NetworkBehaviour
    {
        public Transform spawnLocation;

        private void Awake()
        {
            if (!IsServer)
            {
                enabled = false;
            }
        }

        public abstract void DespawnObject(MoveableObject objectToDespawn);
        
        public abstract void SpawnObject(string path = null);
        
        public void SpawnSomeIngredients()
        {
            var ingredientTypes = Enum.GetValues(typeof(IngredientType));
            foreach (var type in ingredientTypes)
            {
                for(var i = 0; i < 5; i++){
                    var prefab = Resources.Load<GameObject>("Prefabs/Food/Ingredients/" + type);
                    var position = transform.position + (Vector3)Random.insideUnitCircle + Vector3.up;
                    var ingredient = Instantiate(prefab, position, Quaternion.identity);
                    ingredient.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
    }
}

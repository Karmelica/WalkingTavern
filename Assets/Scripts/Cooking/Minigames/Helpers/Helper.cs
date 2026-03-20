using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public abstract class Helper : NetworkBehaviour
    {
        public Transform spawnLocation;

        public abstract void DespawnObject(MoveableObject objectToDespawn);
        
        public abstract void SpawnObjectRpc(string path = null);
        
        public abstract void DespawnObjectRpc(NetworkObjectReference objectReference);
        
        
        public void SpawnSomeIngredients()
        {
            if (!IsServer) return;
            var ingredientTypes = Enum.GetValues(typeof(IngredientType));
            foreach (var type in ingredientTypes)
            {
                for(var i = 0; i < 5; i++){
                    var prefab = Resources.Load<GameObject>("Prefabs/Food/Ingredients/" + type);
                    var position = transform.position + new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 3f,
                        UnityEngine.Random.Range(0f, 8f));
                    var ingredient = Instantiate(prefab, position, Quaternion.identity);
                    ingredient.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
    }
}

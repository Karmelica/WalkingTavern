using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace World
{
    [DefaultExecutionOrder(-50)]
    public class FoodStorage : NetworkBehaviour
    {
        public static FoodStorage Instance;
        private Dictionary<IngredientType, int> _gatheredIngredients = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (Instance != null || !IsServer)
            {
                Destroy(gameObject);
            }
            else{
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public bool GetIngredient(IngredientType requestedIngredient)
        {
            if(_gatheredIngredients.TryGetValue(requestedIngredient, out var value))
            {
                if (value > 0)
                {
                    _gatheredIngredients[requestedIngredient]--;
                    return true;
                }
            }
            return false;
        }

        public int GetIngredientCount(IngredientType requestedIngredient)
        {
            return _gatheredIngredients.GetValueOrDefault(requestedIngredient, 0);
        }

        public void ReturnIngredient(IngredientType returnedIngredient)
        {
            if(!_gatheredIngredients.TryAdd(returnedIngredient, 1))
            {
                _gatheredIngredients[returnedIngredient]++;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace World
{
    public class FoodStorage : NetworkBehaviour
    {
        public static FoodStorage Instance;
        private Dictionary<IngredientType, int> _gatheredIngredients = new Dictionary<IngredientType, int>();
        [SerializeField] private int ingredientQuantity = 10;

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
            //GenerateFood();
        }

        private void GenerateFood()
        {
            foreach (IngredientType ingredientType in Enum.GetValues(typeof(IngredientType)))
            {
                for(int i = 0; i < ingredientQuantity; i++){
                    ReturnIngredient(ingredientType);
                }
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
            _gatheredIngredients.TryGetValue(requestedIngredient, out var value);
            return value;
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

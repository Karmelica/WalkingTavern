using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace World.Caravan
{
    public class FoodStorage : MonoBehaviour
    {
        public static FoodStorage Instance;
        private Dictionary<IngredientType, int> _gatheredIngredients = new();

        private void Awake()
        {
            if(Instance != null){
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
        
        public void ReturnIngredient(IngredientType returnedIngredient)
        {
            if(!_gatheredIngredients.TryAdd(returnedIngredient, 1))
            {
                _gatheredIngredients[returnedIngredient]++;
            }
        }

        public int GetIngredientCount(IngredientType requestedIngredient)
        {
            return _gatheredIngredients.GetValueOrDefault(requestedIngredient, 0);
        }
    }
}

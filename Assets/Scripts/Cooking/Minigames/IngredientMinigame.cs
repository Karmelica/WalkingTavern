using System.Linq;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames
{
    public abstract class IngredientMinigame : Minigame
    {
        [SerializeField] private IngredientType[] applicableFood;
        
        private void OnTriggerEnter(Collider other)
        {
            if (CurrentFood.Any()) return;
            if (!other.gameObject.TryGetComponent(out FoodItem foodItem)) return;
            if (applicableFood.Any(applicableFoodItem => applicableFoodItem == foodItem.ingredientType))
            {
                CurrentFood.Add(foodItem);
                CurrentFood[0].transform.position = foodPlaceholder.position;
                if (NetworkManager.Singleton.IsServer) CurrentFood[0].PlaceOnMinigameRpc();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!CheckForIngredients()) return;
            if (CurrentFood[0] == other.gameObject.GetComponent<FoodItem>())
            {
                RemoveFood();
            }
        }
        
        protected override bool CheckForIngredients()
        {
            return CurrentFood.Any();
        }

        protected override void CompleteMinigame()
        {
            Helper.DespawnObject(CurrentFood[0]);
        }

        protected override void RemoveFood()
        {
            CurrentFood.Clear();
            Score = 0;
        }
    }
}

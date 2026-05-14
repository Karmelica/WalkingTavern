using System.Linq;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames
{
    public abstract class IngredientMinigame : Minigame
    {
        [SerializeField] private IngredientType[] applicableFood;

        protected override void Awake()
        {
            ShowCursor = false;
            base.Awake();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CurrentFood.Any()) return;
            if (!other.gameObject.TryGetComponent(out FoodItem foodItem)) return;
            if (applicableFood.Any(ingredientType => ingredientType == foodItem.ingredientType))
            {
                CurrentFood.Add(foodItem);
                CurrentFood[0].transform.position = new Vector3(foodPlaceholder.position.x, CurrentFood[0].transform.position.y, foodPlaceholder.position.z);
                CurrentFood[0].isOnMinigame = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!CheckForIngredients()) return;
            if (CurrentFood[0] == other.gameObject.GetComponent<FoodItem>())
            {
                CurrentFood[0].isOnMinigame = false;
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
        }
    }
}

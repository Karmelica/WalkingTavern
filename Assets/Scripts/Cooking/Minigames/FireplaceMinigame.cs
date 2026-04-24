using System.Linq;
using UnityEngine;
using World;

namespace Cooking.Minigames
{
    public class FireplaceMinigame : Minigame
    {
        protected override void Update()
        {
            if(IsServer)
                DoMinigame();
        }

        protected override void DoMinigame()
        {
            foreach (var foodItem in CurrentFood)
            {
                if(foodItem is DishItem food)
                    food.CookRpc();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.TryGetComponent(out DishItem dishItem)) return;
            
            CurrentFood.Add(dishItem);
            dishItem.isOnMinigame = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if(!other.gameObject.TryGetComponent(out DishItem otherItem)) return;
            if (CurrentFood.Contains(otherItem))
            {
                otherItem.isOnMinigame = false;
                CurrentFood.Remove(otherItem);
            }
        }
        
        protected override bool CheckForIngredients()
        {
            return CurrentFood.Any();
        }

        protected override void CompleteMinigame()
        {
        }

        protected override void RemoveFood()
        {
        }

        public override string GetInteractText()
        {
            return "Fireplace";
        }
    }
}

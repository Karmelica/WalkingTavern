using System.Linq;
using UnityEngine;
using World;

namespace Cooking.Minigames
{
    public class FireplaceMinigame : Minigame
    {
        [SerializeField] private DishType[] applicableFood;

        protected override void Update()
        {
            if(IsServer)
                DoMinigame();
        }

        protected override void DoMinigame()
        {
            foreach (var foodItem in CurrentFood)
            {
                var food = (DishItem)foodItem;
                food.CookRpc();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.TryGetComponent(out DishItem dishItem)) return;
            if (applicableFood.Any(applicableFoodItem => applicableFoodItem == dishItem.dishType))
            {
                CurrentFood.Add(dishItem);
                dishItem.isOnMinigame = true;
            }
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

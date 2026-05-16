using System;
using System.Linq;
using Managers;
using UnityEngine;
using World;

namespace Cooking.Minigames
{
    public class FireplaceMinigame : Minigame
    {
        protected override void Awake()
        {
            base.Awake();
            AudioManager.Instance.StartFireplace(transform.position);
        }

        protected override void Update()
        {
            if(IsServer)
                DoMinigame();
        }

        protected override void DoMinigame()
        {
            if (!IsSpawned) return;
            foreach (var foodItem in CurrentFood)
            {
                if(foodItem is DishItem food)
                    food.CookRpc();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            AddCollidedFood(other);
        }

        private void AddCollidedFood(Collider other)
        {
            if (!other.gameObject.TryGetComponent(out DishItem dishItem)) return;
            dishItem.OnObjectDisable += RemoveCollidedFood;
            CurrentFood.Add(dishItem);
            dishItem.isOnMinigame = true;
        }

        private void OnTriggerExit(Collider other)
        {
            RemoveCollidedFood(other);
        }

        private void RemoveCollidedFood(Collider other)
        {
            if(!other.gameObject.TryGetComponent(out DishItem otherItem)) return;
            if (CurrentFood.Contains(otherItem))
            {
                otherItem.OnObjectDisable -= RemoveCollidedFood;
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

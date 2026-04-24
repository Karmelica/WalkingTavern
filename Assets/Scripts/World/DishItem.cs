using System;
using Cooking;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace World
{
    public class DishItem : MoveableObject
    {
        public DishType dishType;
        private float _cookingProgressMin;
        private float _cookingProgressMax;
        private NetworkVariable<float> _cookingProgress = new();
        [SerializeField] private TextMeshProUGUI progressText;

        private void OnValidate()
        {
            gameObject.name = dishType.ToString();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            var recipe = GetFoodItems.GetRecipeByDishType(dishType);
            _cookingProgressMin = recipe.cookingMinMax.x;
            _cookingProgressMax = recipe.cookingMinMax.y;
        }

        protected override void Update()
        {
            progressText.text = $"Cooking progress: {_cookingProgress.Value:F0}\nTarget progress: {_cookingProgressMin}~{_cookingProgressMax}";
            base.Update();
        }

        public void Despawn()
        {
            DespawnItemServerRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void DespawnItemServerRpc()
        {
            NetworkObject.Despawn();
        }
        
        [Rpc(SendTo.Server)]
        public void CookRpc()
        {
            _cookingProgress.Value += Time.deltaTime;
        }

        public bool IsCookedEnough()
        {
            return  _cookingProgress.Value <= _cookingProgressMax &&  _cookingProgress.Value >= _cookingProgressMin;
        }
        
        public override string GetInteractText()
        {
            return $"Pick up {Utilis.DeleteAndSplit(dishType.ToString(), "Dish")}";
        }

    }
}

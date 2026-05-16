using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    public class RecipeMenu : NetworkBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private List<DishType> applicableDishes = new();
        public UnityEvent<DishType> onDishTypeChanged;
        private NetworkVariable<DishType> _dishType = new();

        private void Awake()
        {
            dropdown.ClearOptions();
            foreach (var dish in applicableDishes)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(Utilis.DeleteAndSplit(dish.ToString(), "Dish")));
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _dishType.OnValueChanged += OnDishChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _dishType.OnValueChanged -= OnDishChanged;
        }

        private void OnDishChanged(DishType previousValue, DishType newValue)
        {
            dropdown.value = applicableDishes.IndexOf(newValue);
            onDishTypeChanged?.Invoke(newValue);
        }

        public void DropdownValueChanged(int index)
        {
            var type = dropdown.options[dropdown.value].text;
            var replace = type.Replace(" ", "");
            replace = "Dish" + replace;
            var dishType = (DishType)Enum.Parse(typeof(DishType), replace); 
            RecipeChangedRpc(dishType);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RecipeChangedRpc(DishType type)
        {
            _dishType.Value = type;
        }
    }
}

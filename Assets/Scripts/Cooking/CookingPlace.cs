using System;
using System.Collections.Generic;
using Cooking.ScriptableObjects;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking
{
    public class CookingPlace : NetworkBehaviour
    {
        private readonly Dictionary<IngredientType, int> _placedIngredients = new();
        private List<Recipe> _availableRecipes = new ();
        private NetworkVariable<int> _selectedRecipe = new ();
        [SerializeField] private List<GameObject> _placedFoodItems = new();
        [SerializeField] private Recipe recipe;
        
        [SerializeField] private TextMeshProUGUI ingredientListText;
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private Collider triggerCollider;

        private void Start()
        {
            foreach (var ingredientType in Enum.GetValues(typeof(IngredientType)))
            {
                _placedIngredients.TryAdd((IngredientType)ingredientType, 0);
            }
            UpdateRecipeText();

            if (IsServer)
                SpawnSomeIngredients();
            
            _selectedRecipe.OnValueChanged += UpdateSelectedRecipe;
        }

        private void UpdateSelectedRecipe(int previousValue, int newValue)
        {
            recipe = _availableRecipes[newValue];
            dropdown.value = newValue;
            UpdateRecipeText();
        }

        private void Awake()
        {
            LoadRecipes();
        }

        private void LoadRecipes()
        {
            var recipes = Resources.LoadAll<Recipe>("ScriptableObjects/Cooking");
            _availableRecipes.AddRange(recipes);
            recipe = _availableRecipes[0];
            UpdateRecipeText();
        }

        private void SpawnSomeIngredients()
        {
            var ingredientTypes = Enum.GetValues(typeof(IngredientType));
            foreach (var type in ingredientTypes)
            {
                for(var i = 0; i < 5; i++){
                    var prefab = Resources.Load<GameObject>("Prefabs/Food/Ingredients/" + type);
                    var position = transform.position + new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 3f,
                        UnityEngine.Random.Range(0f, 8f));
                    var ingredient = Instantiate(prefab, position, Quaternion.identity);
                    ingredient.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
        
        public void ChangeRecipe(int newRecipeIndex)
        {
            ChangeRecipeServerRpc(newRecipeIndex);
            recipe = _availableRecipes[newRecipeIndex];
            UpdateRecipeText();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ChangeRecipeServerRpc(int newRecipeIndex)
        {
            _selectedRecipe.Value = newRecipeIndex;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<FoodItem>(out var foodItem)) return;
            if (!_placedIngredients.TryAdd(foodItem.ingredientType, 1))
            {
                _placedIngredients[foodItem.ingredientType]++;
                if (!_placedFoodItems.Contains(other.gameObject))
                {
                    _placedFoodItems.Add(other.gameObject);
                }
            }
            UpdateRecipeText();
            
            if (!IsRecipeComplete()) return;
            
            //enable skillcheck here
            CompleteRecipe();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<FoodItem>(out var foodItem)) return;
            if (!_placedIngredients.TryGetValue(foodItem.ingredientType, out var ingredient)) return;
            if (_placedFoodItems.Contains(other.gameObject))
            { 
                _placedFoodItems.Remove(other.gameObject);
            }
            if (ingredient <= 0)
            {
                _placedIngredients.Remove(foodItem.ingredientType);
                return;
            }
            _placedIngredients[foodItem.ingredientType]--;
                
            UpdateRecipeText();
        }

        private void CompleteRecipe()
        {
            foreach (var foodItem in recipe.ingredients)
            {
                var ingredientType = foodItem.ingredient;
                var quantity = foodItem.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) continue;
                if (placedCount < quantity) continue;

                int removedCount = 0;
                for (int i = _placedFoodItems.Count - 1; i >= 0 && removedCount < quantity; i--)
                {
                    var item = _placedFoodItems[i];
                    if (item.TryGetComponent<FoodItem>(out var foodItemComponent) && foodItemComponent.ingredientType == ingredientType)
                    {
                        item.gameObject.SetActive(false);
                        _placedFoodItems.RemoveAt(i);
                        removedCount++;
                        //if (IsServer) item.GetComponent<NetworkObject>().Despawn(false);
                    }
                }
                _placedIngredients[ingredientType] -= removedCount;
            }
            
            if(IsServer){
                var prefab = Resources.Load<GameObject>("Prefabs/Food/Dishes/" + recipe.dishType);
                var dish = Instantiate(prefab, transform.position + Vector3.up * 5.0f, Quaternion.identity);
                dish.GetComponent<NetworkObject>().Spawn();
            }
            UpdateRecipeText();
        }
        
        private void UpdateRecipeText()
        {
            ingredientListText.text = "Ingredients:\n";
            foreach (var ingredient in recipe.ingredients)
            {
                var ingredientType = ingredient.ingredient;
                var ingredientQuantity = ingredient.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) continue;
                ingredientListText.text += $"{placedCount}/{ingredientQuantity} of {ingredientType}\n";
            }
        }
        
        private bool IsRecipeComplete()
        {
            foreach (var ingredient in recipe.ingredients)
            {
                var ingredientType = ingredient.ingredient;
                var ingredientQuantity = ingredient.quantity;

                if (!_placedIngredients.TryGetValue(ingredientType, out var placedCount)) return false;
                
                if (placedCount < ingredientQuantity)
                {
                    return false;
                }
            }
            return true;

            //recipe complete
            //skillCheck.gameObject.SetActive(true);
        }
    }
}

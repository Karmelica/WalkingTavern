using System.Collections.Generic;
using Cooking.Minigames;
using Cooking.ScriptableObjects;
using PlayerScripts;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Cooking
{
    public class CookingMenu : MonoBehaviour, IInteractable
    {
        private readonly List<Recipe> _availableRecipes = new ();
        [SerializeField] private List<Recipe> selectedRecipes = new ();
        [SerializeField] private TMP_Dropdown recipeDropdown;
        [SerializeField] private DishMinigame dishMakingPlace;
        [SerializeField] private GameObject cookingUI;
    
        private void Awake()
        {
            if(!dishMakingPlace) dishMakingPlace = GetComponentInParent<DishMinigame>();
            LoadRecipes();
        }

        private void LoadRecipes()
        {
            _availableRecipes.AddRange(selectedRecipes);
            //dishMakingPlace.AddRecipes(selectedRecipes);

            foreach (var recipe in _availableRecipes)
            {
                recipeDropdown.options.Add(new TMP_Dropdown.OptionData(recipe.recipeName));
            }
        }
    
        public void OnRecipeSelected()
        {
            //dishMakingPlace.ChangeRecipe(recipeDropdown.value);
        }


        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
        {
            if(!startedInteraction)
                recipeDropdown.Hide();
            return null;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor)
        {
            interactor.SetCanMove(!interactor.CanMove());

            return this;
        }

        public string GetInteractName()
        {
            return gameObject.name;
        }

        public bool IsInteractedWith()
        {
            return false;
        }
    }
}
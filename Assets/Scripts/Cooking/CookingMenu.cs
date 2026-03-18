using System.Collections.Generic;
using Cooking.ScriptableObjects;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Cooking
{
    public class CookingMenu : MonoBehaviour, IInteractable
    {
    
        [SerializeField] private List<Recipe> availableRecipes = new ();
        [SerializeField] private TMP_Dropdown recipeDropdown;
        [SerializeField] private CookingPlace cookingPlace;
        [SerializeField] private GameObject cookingUI;
    
        private void Awake()
        {
            LoadRecipes();
        }

        private void LoadRecipes()
        {
            var recipes = Resources.LoadAll<Recipe>("ScriptableObjects/Cooking");
            availableRecipes.AddRange(recipes);

            foreach (var recipe in availableRecipes)
            {
                recipeDropdown.options.Add(new TMP_Dropdown.OptionData(recipe.recipeName));
            }
        }
    
        public void OnRecipeSelected()
        {
            cookingPlace.ChangeRecipe(recipeDropdown.value);
        }


        public IInteractable PrimaryInteract(NetworkBehaviourReference interactor, bool beingPickedUp = true)
        {
            if (interactor.TryGet(out PlayerScripts.OwnerPlayer player))
            {
                cookingUI.SetActive(!player.CanMove());
            }
            
            return null;
        }

        public IInteractable SecondaryInteract(NetworkBehaviourReference interactor)
        {
            if (interactor.TryGet(out PlayerScripts.OwnerPlayer player))
            {
                player.SetCanMove(!player.CanMove());
                cookingUI.SetActive(!player.CanMove());
            }

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
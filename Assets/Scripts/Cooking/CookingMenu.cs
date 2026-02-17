using System.Collections.Generic;
using Cooking;
using Cooking.ScriptableObjects;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CookingMenu : MonoBehaviour, IInteractable
{
    
    [SerializeField] private List<Recipe> _availableRecipes = new ();
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
        _availableRecipes.AddRange(recipes);

        foreach (var recipe in _availableRecipes)
        {
            recipeDropdown.options.Add(new TMP_Dropdown.OptionData(recipe.recipeName));
        }
    }
    
    public void OnRecipeSelected()
    {
        cookingPlace.ChangeRecipe(recipeDropdown.value);
    }


    public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
    {
        //nothing
    }

    public void SecondaryInteract(NetworkBehaviourReference interactor)
    {
        if (!interactor.TryGet(out Player.Player player)) return;
        
        player.SetCanMove(!player.CanMove());
        cookingUI.SetActive(!player.CanMove());
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
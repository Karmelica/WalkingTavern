using System.Collections.Generic;
using Cooking;
using Cooking.ScriptableObjects;
using TMPro;
using UnityEngine;

public class CookingMenu : MonoBehaviour
{
    [SerializeField] private List<Recipe> _availableRecipes = new ();
    [SerializeField] private TMP_Dropdown recipeDropdown;
    [SerializeField] private CookingPlace cookingPlace;
    
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
    
    public void OnRecipeSelected(int index)
    {
        cookingPlace.ChangeRecipe(_availableRecipes[index]);
    }
}
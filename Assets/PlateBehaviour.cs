using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateBehaviour : MonoBehaviour {
    System.String newIngredient;
    List<System.String> ingredients = null;
    Dictionary<System.String, List<System.String>> recipes;
    bool validRecipe = false;
    System.String recipe = null;

	// Use this for initialization
	void Start () {
        // Set new ingredient to the ingredient being held by the player
		// Send initial message to server
        // Retrieve ingredients currently on plate from server
        // Check if a recipe is on the plate
        // Display current ingredients/recipe on plate
	}
	
    void checkRecipe()
    {
        // Loop through each recipe in the recipe dictionary
        foreach (KeyValuePair<System.String, List<System.String>> item in recipes)
        {
            bool matches = true;

            // Check each ingredient in each recipe against each ingredient stored on the plate
            // Finish early if all ingredients on the plate match a recipe
            foreach (var ingredient1 in item.Value)
            {
                foreach (var ingredient2 in ingredients)
                {
                    if (ingredient1 != ingredient2)
                    {
                        matches = false;
                        break;
                    }
                }
                if (matches == false) break;
            }
            if (matches == true)
            {
                recipe = item.Key;
                break;
            }
        }

        displayFood();

        if (recipe == null) validRecipe = false;
        else validRecipe = true;
    }

    void displayFood()
    {
        if (ingredients == null)
        {
            // Display no food on plate
        }
        else if (recipe == null)
        {
            // Display shit food
        }
        else
        {
            // Display correct recipe
        }
    }

    public void serveFood()
    {
        // If recipe is valid, calculates a score and sends back to the server
        if (validRecipe)
        {
            if (recipe == "chips") sendToServer("1");
            else if (recipe == "stirfry") sendToServer("2");
            else if (recipe == "pancakes") sendToServer("3");
        }
        // Else if the recipe is invalid, sends a score of 0 back to the server
        else
        {
            sendToServer("0");
        }
    }

    public void addIngredient()
    {
        // Adds the ingredient the player is holding to the ingredients list
        ingredients.Add(newIngredient);
        // Check if there is now a recipe present and display the resulting food
        checkRecipe();
    }

    void sendToServer(System.String message)
    {

    }
}

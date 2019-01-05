using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

public class PlateBehaviour : MonoBehaviour {
    // Ingredient the player is holding
    string newIngredient;
    // All ingredients on the plate
    IList<string> ingredients;
    // Dictionary of all the recipes
    Dictionary<string, List<string>> recipes = new Dictionary<string, List<string>>() { { "pancakes", new List<string> { "flour", "milk", "eggs" } } };
    // True when a valid recipe is on the plate
    bool validRecipe = false;
    // Holds the name of the recipe
    string recipe = null;
    // The text list of ingredients to be displayed
    Text ingList;

    // Cameras
    Camera cameraEmpty;
    Camera cameraMush;
    Camera cameraPancake;

    // Networking variables
    private const int MAX_CONNECTION = 10;
    private const string serverIP = "192.168.0.62";

    private int port = 8080;

    private int hostId;

    private int reliableChannel;
    private int unreliableChannel;

    ConnectionConfig connectConfig;

    private int connectionId;

    private bool isConnected = false;

    private byte error;

    // Use this for initialization
    void Start () {
        // Connect to server
        NetworkTransport.Init();
        connectConfig = new ConnectionConfig();

        reliableChannel = connectConfig.AddChannel(QosType.ReliableSequenced);
        HostTopology topo = new HostTopology(connectConfig, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null /*ipAddress*/);
        connectionId = NetworkTransport.Connect(hostId, serverIP, port, 0, out error);

        isConnected = true;

        int recHostId; // Player ID
        int channelID; // ID of channel connected to recHostId.
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;

        // Get all camera objects
        cameraEmpty = GameObject.Find("Camera1").GetComponent<Camera>();
        cameraMush = GameObject.Find("Camera2").GetComponent<Camera>();
        cameraPancake = GameObject.Find("Camera3").GetComponent<Camera>();

        // Set new ingredient to the ingredient being held by the player
        // TODO

        // Send initial message to server (plate id from NFC tag)
        // TODO
        sendToServer("My unique tag");

        // Retrieve ingredients currently on plate from server
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelID,
                                                            recBuffer, bufferSize, out dataSize, out error);

        switch (recData)
        {
            case NetworkEventType.Nothing: break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player " + connectionId + " has been connected to server.");
                break;
            case NetworkEventType.DataEvent:
                string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Server has sent: " + message);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player " + connectionId + " has been disconnected to server");
                break;
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Broadcast event.");
                break;
        }

        // Split up string of ingredients into list
        // TODO
        ingredients = new List<string>() { };

        ingList = GameObject.Find("Ingredient List").GetComponent<Text>();

        // Check if a recipe is on the plate
        // Display current ingredients/recipe on plate
        checkRecipe();
	}
	
    void checkRecipe()
    {
        // Loop through each recipe in the recipe dictionary
        foreach (KeyValuePair<string, List<string>> item in recipes)
        {
            bool matches = true;

            // Check each ingredient in each recipe against each ingredient stored on the plate
            // Finish early if all ingredients on the plate match a recipe
            foreach (var ingredient1 in item.Value)
            {
                bool ingFound = false;
                foreach (var ingredient2 in ingredients)
                {
                    if (ingredient1 == ingredient2)
                    {
                        ingFound = true;
                        break;
                    }
                }
                if (!ingFound)
                {
                    matches = false;
                    break;
                }
            }
            if (matches == true)
            {
                recipe = item.Key;
                break;
            }
        }

        displayFood();

        // No valid recipe was found so just set the list of ingredients
        if (recipe == null)
        {
            validRecipe = false;
            string ingText = "Current Ingredients:\n";
            foreach (var ingredient in ingredients)
            {
                ingText += ingredient + "\n";
            }
            ingList.text = ingText;
        }
        // A valid recipe was found so tell the player
        else
        {
            validRecipe = true;
            string ingText = "Current Recipe:\n" + recipe;
            ingList.text = ingText;
        }
    }

    void displayFood()
    {
        if (ingredients.Count == 0)
        {
            // Display no food on plate
            cameraEmpty.enabled = true;
            cameraMush.enabled = false;
            cameraPancake.enabled = false;
        }
        else if (recipe == null)
        {
            // Display shit food
            cameraEmpty.enabled = false;
            cameraMush.enabled = true;
            cameraPancake.enabled = false;
        }
        else
        {
            // Display correct recipe
            if (recipe == "pancakes")
            {
                cameraEmpty.enabled = false;
                cameraMush.enabled = false;
                cameraPancake.enabled = true;
            }
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

    void sendToServer(string textInput)
    {
        byte error;
        byte[] buffer = new byte[1024];
        Stream message = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        //Serialize the message
        formatter.Serialize(message, textInput);

        //Send the message from the "client" with the serialized message and the connection information
        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, (int)message.Position, out error);

        //If there is an error, output message error to the console
        if ((NetworkError)error != NetworkError.Ok)
        {
            Debug.Log("Message send error: " + (NetworkError)error);
        }
    }
}

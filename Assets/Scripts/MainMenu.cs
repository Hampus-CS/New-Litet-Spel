using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

// Handles the main menu functionality, including starting a new game and loading a saved game.
public class MainMenu : MonoBehaviour
{
    // File Path
    private string savePath;

    // UI Elements
    [Header("UI Elements")]
    public Button loadButton; // Button to load a saved game
    public Button playButton; // Button to start a new game

    protected void Awake()
    {
        savePath = Application.persistentDataPath + "/savegame.json";
    }

    protected void Start()
    {
        Debug.Log($"Save directory is: {Application.persistentDataPath}");

        playButton.interactable = true; // Always enable the Play button

        // Enable or disable the Load button based on save file existence
        if (File.Exists(savePath))
        {
            loadButton.interactable = true;
            Debug.Log("Save file found. Load button enabled.");
        }
        else
        {
            loadButton.interactable = false;
            Debug.Log("No save file found. Load button disabled.");
        }
    }

    // Starts a new game, ensuring a fresh state.
    public void OnPlayButtonClick()
    {
        GameLoadManager.ShouldLoadGame = false; // Set flag for a fresh game
        SceneManager.LoadScene("GameScene");   // Replace "GameScene" with the actual scene name
    }

    // Loads a saved game if a save file exists.
    public void OnLoadGameButtonClick()
    {
        if (File.Exists(savePath))
        {
            GameLoadManager.ShouldLoadGame = true; // Set flag to load saved data
            SceneManager.LoadScene("GameScene");   // Replace "GameScene" with the actual scene name
            Debug.Log("Save file found. Loading game...");
        }
        else
        {
            Debug.LogWarning("No save file exists. Cannot load the game.");
        }
    }
}
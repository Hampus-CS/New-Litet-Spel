using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainMenu : MonoBehaviour
{
    private string savePath;

    [Header("UI Elements")]
    public Button loadButton; // Assign this in the Inspector to the Load button
    public Button playButton; // Assign this in the Inspector to the Play button

    private void Awake()
    {
        // Path where the save file is stored
        savePath = Application.persistentDataPath + "/savegame.json";
    }

    private void Start()
    {
        Debug.Log($"Save directory is: {Application.persistentDataPath}");

        // Always enable the Play button
        playButton.interactable = true;

        // Disable the Load button if no save file exists
        if (!File.Exists(savePath))
        {
            loadButton.interactable = false; // Make the Load button unclickable
            Debug.Log("No save file found. Load button disabled.");
        }
        else
        {
            loadButton.interactable = true; // Ensure Load button is interactable if save exists
            Debug.Log("Save file found. Load button enabled.");
        }
    }

    public void OnPlayButtonClick()
    {
        // Start a new game
        GameLoadManager.ShouldLoadGame = false; // Ensure a fresh game
        SceneManager.LoadScene("GameScene"); // Replace "GameScene" with your actual Game Scene name
    }

    public void OnLoadGameButtonClick()
    {
        // Check if the save file exists before loading
        if (File.Exists(savePath))
        {
            GameLoadManager.ShouldLoadGame = true; // Set the flag to load saved data
            SceneManager.LoadScene("GameScene");   // Replace "GameScene" with your actual Game Scene name
            Debug.Log("Save file found. Loading game...");
        }
        else
        {
            Debug.LogWarning("No save file exists. Cannot load the game.");
        }
    }
}
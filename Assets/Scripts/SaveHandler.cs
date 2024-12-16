using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Handles saving and loading the game state to/from a JSON file.
public class SaveHandler : MonoBehaviour
{
    // File Path
    protected string savePath;

    protected void Awake()
    {
        savePath = Application.persistentDataPath + "/savegame.json";
    }

    // Saves the current game state to a JSON file.
    public void Save(GameManager gameManager, TimeManager timeManager, ManpowerManager manpowerManager, MoraleManager moraleManager)
    {
        GameState state = new GameState
        {
            CurrentDay = gameManager.GetCurrentDay(),
            CurrentTime = timeManager.GetCurrentTime(),
            Manpower = manpowerManager.GetCurrentManpower(),
            MoraleSummary = moraleManager.GetSummary(),
            Sectors = gameManager.GetSectorsState(),
            Soldiers = gameManager.GetSoldiersState(),
            Enemies = gameManager.GetEnemiesState()
        };

        string json = JsonUtility.ToJson(state, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game saved successfully.");
    }

    // Loads the game state from a JSON file.
    public GameState Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            GameState state = JsonUtility.FromJson<GameState>(json);
            Debug.Log("Game loaded successfully.");
            return state;
        }

        Debug.LogWarning("Save file not found.");
        return null;
    }
}

/// <summary>
/// Serializable Classes for Game State
/// </summary>

// Represents the entire game state for saving/loading.
[System.Serializable]
public class GameState
{
    public int CurrentDay;
    public float CurrentTime;
    public int Manpower;
    public int MoraleSummary;
    public List<SectorState> Sectors;
    public List<SoldierState> Soldiers;
    public List<EnemyState> Enemies;
}

// Represents the state of a sector.
[System.Serializable]
public class SectorState
{
    public string Name;
    public bool Controlled;
    public int Owner;
}

// Represents the state of a soldier.
[System.Serializable]
public class SoldierState
{
    public string Type; // "Basic" or "Advanced"
    public float[] Position;
    public int Health;
    public int Morale;
}

// Represents the state of an enemy.
[System.Serializable]
public class EnemyState
{
    public float[] Position;
    public int Health;
}
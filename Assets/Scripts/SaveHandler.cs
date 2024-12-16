using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SaveHandler : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        savePath = Application.persistentDataPath + "/savegame.json";
    }

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

[System.Serializable]
public class SectorState
{
    public string Name;
    public bool Controlled;
    public int Owner;
}

[System.Serializable]
public class SoldierState
{
    public string Type; // e.g., "Basic" or "Advanced"
    public float[] Position;
    public int Health;
    public int Morale;
}

[System.Serializable]
public class EnemyState
{
    public float[] Position;
    public int Health;
}
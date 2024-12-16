using System.IO;
using UnityEngine;

public class SaveHandler : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        // Initialize savePath here instead of at field declaration
        savePath = Application.persistentDataPath + "/savegame.json";
    }

    public void Save(int currentDay, float currentTime, int manpower, int moraleSummary)
    {
        GameState state = new GameState
        {
            CurrentDay = currentDay,
            CurrentTime = currentTime,
            Manpower = manpower,
            MoraleSummary = moraleSummary
        };
        string json = JsonUtility.ToJson(state);
        File.WriteAllText(savePath, json);
    }

    public GameState Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<GameState>(json);
        }
        return null;
    }
}

public class GameState
{
    public int CurrentDay;
    public float CurrentTime;
    public int Manpower;
    public int MoraleSummary;
}
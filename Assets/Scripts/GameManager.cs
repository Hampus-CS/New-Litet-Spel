using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Game Settings
    public Light2D globalLight;
    public int totalDays = 15;
    public int supportDays = 10;

    // Managers
    private int currentDay = 1;
    private TimeManager timeManager;
    private SaveHandler saveHandler;
    private ManpowerManager manpowerManager;
    private MoraleManager moraleManager;

    // Game Entities
    private List<Sector> sectors = new List<Sector>();
    private List<ISoldier> soldiers = new List<ISoldier>();
    private List<Enemy> enemies = new List<Enemy>();

    public GameObject basicSoldierPrefab;
    public GameObject advancedSoldierPrefab;
    public GameObject enemyPrefab;
    public Transform spawnParent; // Parent for spawned units

    private Sector currentPlayerSector;

    // UI Elements
    public TMP_Text manpowerText;
    public Button basicSoldierButton;
    public Button advancedSoldierButton;

    public GameObject dayEndPopup;  // Popup for day-end messages
    public TMP_Text dayEndText;     // TextMeshPro for day-end messages

    private void Start()
    {
        // Initialize managers
        timeManager = FindFirstObjectByType<TimeManager>();
        saveHandler = FindFirstObjectByType<SaveHandler>();
        moraleManager = FindFirstObjectByType<MoraleManager>();
        manpowerManager = FindFirstObjectByType<ManpowerManager>();

        if (timeManager == null || saveHandler == null || moraleManager == null || manpowerManager == null)
        {
            Debug.LogError("One or more manager components are missing!");
            return;
        }

        InitializeSectors(10); // Initialize sectors (10 tiles high each)
        currentPlayerSector = sectors[0]; // Ensure the first sector is active
        Debug.Log($"Initial active sector: {currentPlayerSector.Name}");

        // Set up UI buttons
        basicSoldierButton.onClick.AddListener(() => PurchaseSoldier("Basic"));
        advancedSoldierButton.onClick.AddListener(() => PurchaseSoldier("Advanced"));

        SpawnEnemiesAtDayStart(10);

        UpdateManpowerUI();
        StartCoroutine(DayNightCycle());
    }

    private void InitializeSectors(int sectorHeight)
    {
        string[] sectorNames = { "Start", "A", "B", "C", "D", "E", "Finish" };

        for (int i = 0; i < sectorNames.Length; i++)
        {
            Vector3 position = new Vector3(0, i * sectorHeight, 0);
            sectors.Add(new Sector(sectorNames[i])
            {
                Controlled = i == 0,
                Owner = i == 0 ? 1 : 2,
                Position = position
            });

            Debug.Log($"Sector {sectorNames[i]} initialized at position {position}");
        }
    }

    private IEnumerator DayNightCycle()
    {
        while (currentDay <= totalDays)
        {
            if (!timeManager.isTimeFrozen && timeManager.AdvanceTime())
            {
                timeManager.isTimeFrozen = true; // Stop time progression
                yield return ShowDayEndPopup();

                // Reset for the new day
                currentDay++;
                HandleNewDay(); // Call to handle the new day
                timeManager.ResetTime();
                timeManager.isTimeFrozen = false; // Resume time progression
            }

            if (!timeManager.isTimeFrozen)
            {
                UpdateLighting();
                SimulateCombat();
            }

            yield return null;
        }
    }

    private IEnumerator ShowDayEndPopup()
    {
        dayEndText.text = $"Day {currentDay} has ended. Welcome to Day {currentDay + 1}!";
        dayEndPopup.SetActive(true);
        Debug.Log($"Popup displayed for Day {currentDay}");
        yield return new WaitForSeconds(10f); // Wait for the popup duration
        dayEndPopup.SetActive(false);
        Debug.Log("Popup hidden.");
    }

    private void HandleNewDay()
    {
        manpowerManager.CalculateManpower(moraleManager.GetSummary());
        moraleManager.ResetSummary();
        UpdateManpowerUI();

        int enemiesToSpawn = 10; // Fixed amount of enemies to spawn
        SpawnEnemiesAtDayStart(enemiesToSpawn);

        Debug.Log($"New Day: {currentDay}. {enemiesToSpawn} enemies spawned at {currentPlayerSector.Name}.");
    }

    private void UpdateLighting()
    {
        timeManager.UpdateLighting();
    }

    private void SimulateCombat()
    {
        foreach (var soldier in soldiers)
        {
            soldier.PerformAction(enemies);
            AttemptSectorControl(soldier);
        }

        foreach (var enemy in enemies)
        {
            enemy.PerformAction(soldiers);
        }
    }

    private void AttemptSectorControl(ISoldier soldier)
    {
        foreach (var sector in sectors)
        {
            if (!sector.Controlled && Vector3.Distance(soldier.Position, sector.GetBottomPosition()) < 1f)
            {
                sector.Controlled = true;
                sector.Owner = 1;
                currentPlayerSector = sector;
                Debug.Log($"Sector {sector.Name} claimed by the player.");
            }
        }
    }

    private void SpawnEnemiesAtDayStart(int amount)
    {
        if (currentPlayerSector == null)
        {
            Debug.LogError("currentPlayerSector is null! Cannot spawn enemies.");
            return;
        }

        Vector3 topPosition = currentPlayerSector.GetTopPosition();
        Debug.Log($"Spawning enemies at the TOP of sector {currentPlayerSector.Name}, position: {topPosition}");

        for (int i = 0; i < amount; i++)
        {
            // Adjust to spawn more to the left
            Vector3 offsetPosition = new Vector3(topPosition.x - (amount * 0.75f) + (i * 1.5f), topPosition.y, -1);

            GameObject enemyObject = Instantiate(enemyPrefab, offsetPosition, Quaternion.identity, spawnParent);

            Enemy enemy = enemyObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemies.Add(enemy);
            }
            else
            {
                Debug.LogError("Enemy prefab is missing the Enemy script component!");
            }
        }
        Debug.Log($"{amount} enemies spawned at the TOP of sector {currentPlayerSector.Name}");
    }

    private Sector GetClosestEnemySector()
    {
        foreach (var sector in sectors)
        {
            if (!sector.Controlled)
            {
                return sector;
            }
        }
        return null;
    }

    private void SpawnEnemy(Vector3 position)
    {
        GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity, spawnParent);
        Enemy newEnemy = enemyObject.GetComponent<Enemy>();
        enemies.Add(newEnemy);
        Debug.Log($"Enemy spawned at {position}");
    }

    private void PurchaseSoldier(string type)
    {
        Vector3 spawnPosition = currentPlayerSector.GetBottomPosition();
        manpowerManager.PurchaseSoldier(type, spawnPosition, soldiers);
        UpdateManpowerUI();
    }

    private void UpdateManpowerUI()
    {
        manpowerText.text = $"Manpower: {manpowerManager.GetCurrentManpower()}";
    }
}

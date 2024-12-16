using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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
    public Camera mainCamera; // Assign the main camera in the Inspector


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

    private IEnumerator MoveCameraToSector(Sector targetSector, float duration)
    {
        if (targetSector == null)
        {
            Debug.LogError("Target sector is null. Cannot move the camera.");
            yield break;
        }

        Vector3 targetPosition = targetSector.Position + new Vector3(0, 0, -10); // Adjust Z for camera depth
        Vector3 startPosition = mainCamera.transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        Debug.Log($"Camera moved to sector: {targetSector.Name}");
    }

    private void MoveSoldiersToSector(Sector targetSector)
    {
        if (targetSector == null)
        {
            Debug.LogError("Target sector is null. Cannot move soldiers.");
            return;
        }

        Vector3 targetPosition = targetSector.GetBottomPosition();

        foreach (var soldier in soldiers)
        {
            soldier.Position = targetPosition; // Update soldier's position
            Debug.Log($"Moved soldier to sector: {targetSector.Name}");
        }
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
        if (AreAllEnemiesDefeatedInSector(currentPlayerSector))
        {
            dayEndText.text = $"Day {currentDay} has ended. Welcome to Day {currentDay + 1}!";
        }
        else
        {
            dayEndText.text = $"Day {currentDay} has ended, but sector {currentPlayerSector.Name} is not fully cleared!";
        }

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
        Debug.Log($"Handled new day: {currentDay}. Manpower updated.");

        // Check if all enemies in the current sector are defeated
        if (!AreAllEnemiesDefeatedInSector(currentPlayerSector))
        {
            Debug.LogWarning($"Day {currentDay}: Cannot move to the next sector. Enemies still present in sector {currentPlayerSector.Name}.");
            return; // Do not proceed to claim the next sector
        }

        // Move to the next sector if possible
        Sector nextPlayerSector = GetClosestEnemySector();
        if (nextPlayerSector != null)
        {
            nextPlayerSector.Controlled = true;
            nextPlayerSector.Owner = 1;
            currentPlayerSector = nextPlayerSector;

            StartCoroutine(MoveCameraToSector(nextPlayerSector, 2f));
            MoveSoldiersToSector(nextPlayerSector);

            int enemiesToSpawn = Mathf.Clamp(currentDay * 2, 10, 30);
            SpawnEnemiesAtDayStart(enemiesToSpawn);

            Debug.Log($"New Day: {currentDay}. Sector {nextPlayerSector.Name} claimed. {enemiesToSpawn} enemies spawned.");
        }
        else
        {
            Debug.LogWarning("No more sectors to claim. Player might have won!");
        }
    }

    private bool AreAllEnemiesDefeatedInSector(Sector sector)
    {
        foreach (var enemy in enemies)
        {
            if (Vector3.Distance(enemy.transform.position, sector.Position) < 10f) // Adjust range as needed
            {
                return false; // An enemy is still within the sector
            }
        }
        return true;
    }

    private void UpdateLighting()
    {
        timeManager.UpdateLighting();
    }

    private void SimulateCombat()
    {
        // Clean up destroyed soldiers
        for (int i = soldiers.Count - 1; i >= 0; i--)
        {
            if (soldiers[i] == null || soldiers[i].Health <= 0)
            {
                Debug.Log($"Soldier removed: {soldiers[i]?.GetType().Name ?? "null"}");
                soldiers.RemoveAt(i);
            }
        }

        // Soldiers attack enemies
        foreach (var soldier in new List<ISoldier>(soldiers))
        {
            soldier.PerformAction(enemies);
        }

        // Clean up destroyed enemies
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null || enemies[i].Health <= 0)
            {
                Debug.Log($"Enemy removed.");
                enemies.RemoveAt(i);
            }
        }

        // Enemies attack soldiers
        foreach (var enemy in new List<Enemy>(enemies))
        {
            enemy.PerformAction(soldiers);
        }
    }

    private void AttemptSectorControl(ISoldier soldier)
    {
        foreach (var sector in sectors)
        {
            if (!sector.Controlled &&
                Vector3.Distance(soldier.Position, sector.GetBottomPosition()) < 1f &&
                AreAllEnemiesDefeatedInSector(sector))
            {
                sector.Controlled = true;
                sector.Owner = 1;
                currentPlayerSector = sector;
                Debug.Log($"Sector {sector.Name} claimed by the player.");
            }
            else if (!sector.Controlled)
            {
                Debug.Log($"Sector {sector.Name} cannot be claimed yet. Enemies still present.");
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

        Debug.Log($"Attempting to spawn {amount} enemies in {currentPlayerSector.Name}");

        Vector3 topPosition = currentPlayerSector.GetTopPosition();
        Debug.Log($"Spawning {amount} enemies at the TOP of sector {currentPlayerSector.Name}, position: {topPosition}");

        for (int i = 0; i < amount; i++)
        {
            Vector3 offsetPosition = new Vector3(
                topPosition.x - (amount * 0.75f) + (i * 1.5f),
                topPosition.y,
                -1
            );

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
        Debug.Log($"{amount} enemies successfully spawned.");
    }

    private Sector GetClosestEnemySector()
    {
        foreach (var sector in sectors)
        {
            if (!sector.Controlled && sector.Owner != 1) // Ensure this logic works
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

    public void RemoveSoldier(ISoldier soldier)
    {
        soldiers.Remove(soldier);
        Debug.Log($"Soldier removed: {soldier.GetType().Name}");
    }

    private void UpdateManpowerUI()
    {
        manpowerText.text = $"Manpower: {manpowerManager.GetCurrentManpower()}";
    }
}

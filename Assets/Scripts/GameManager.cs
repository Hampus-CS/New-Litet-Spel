using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Manages the overall game state, including day progression, combat, and saving/loading.
public class GameManager : MonoBehaviour
{
    // Game Settings
    protected int totalDays = 15;    // Total playable days
    protected int supportDays = 10;  // Days with additional support

    // Day/Night Cycle
    protected int currentDay = 1;    // Current game day
    protected Light2D globalLight;   // Global lighting for day-night simulation

    // Managers
    protected TimeManager timeManager;
    protected SaveHandler saveHandler;
    protected ManpowerManager manpowerManager;
    protected MoraleManager moraleManager;

    // Game Entities
    protected List<Sector> sectors = new List<Sector>();
    protected List<ISoldier> soldiers = new List<ISoldier>();
    protected List<Enemy> enemies = new List<Enemy>();
    protected Sector currentPlayerSector;

    [Header("Prefabs and Spawning")]
    public GameObject basicSoldierPrefab;
    public GameObject advancedSoldierPrefab;
    public GameObject enemyPrefab;
    public Transform spawnParent;

    [Header("UI")]
    public TMP_Text manpowerText;
    public Button basicSoldierButton;
    public Button advancedSoldierButton;
    public GameObject dayEndPopup;
    public TMP_Text dayEndText;

    [Header("Camera")]
    public Camera mainCamera;

    /// <summary>
    /// Initialization
    /// </summary>

    protected void Start()
    {
        InitializeManagers();
        LoadOrInitializeGame();

        // Set up UI button actions
        basicSoldierButton.onClick.AddListener(() => PurchaseSoldier("Basic"));
        advancedSoldierButton.onClick.AddListener(() => PurchaseSoldier("Advanced"));
        
        UpdateManpowerUI();

        StartCoroutine(DayNightCycle());
    }

    private void InitializeManagers()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
        saveHandler = FindFirstObjectByType<SaveHandler>();
        moraleManager = FindFirstObjectByType<MoraleManager>();
        manpowerManager = FindFirstObjectByType<ManpowerManager>();

        if (timeManager == null || saveHandler == null || moraleManager == null || manpowerManager == null)
            Debug.LogError("Missing one or more manager components.");
    }

    // Loads saved game or initializes a new game.
    private void LoadOrInitializeGame()
    {
        if (GameLoadManager.ShouldLoadGame)
        {
            LoadGame();
            GameLoadManager.ShouldLoadGame = false; // Reset load flag
        }
        else
        {
            InitializeSectors(10);
            currentPlayerSector = sectors[0];
            SpawnEnemiesAtDayStart(10);
        }
    }

    /// <summary>
    /// Core Gameplay Methods
    /// </summary>

    protected IEnumerator DayNightCycle()
    {
        while (currentDay <= totalDays)
        {
            if (timeManager.AdvanceTime())
            {
                yield return ShowDayEndPopup();
                StartNewDay();
                timeManager.ResetTime();
            }

            UpdateLighting();
            SimulateCombat();
            yield return null;
        }
    }

    private void StartNewDay()
    {
        currentDay++;

        // Calculate manpower and reset morale
        manpowerManager.CalculateManpower(moraleManager.GetSummary());
        moraleManager.ResetSummary();

        UpdateManpowerUI();

        // Ensure enemies spawn in the new sector
        if (AreAllEnemiesDefeatedInSector(currentPlayerSector))
        {
            Sector nextSector = GetClosestEnemySector();
            if (nextSector != null)
            {
                nextSector.Controlled = true;
                currentPlayerSector = nextSector;

                StartCoroutine(MoveCameraToSector(nextSector, 2f));
                MoveSoldiersToSector(nextSector);

                int enemiesToSpawn = Mathf.Clamp(currentDay * 2, 5, 30); // Scale enemy count with day
                SpawnEnemiesAtDayStart(enemiesToSpawn);

            }
            else
            {
                Debug.Log("No more sectors to claim. Game may be completed.");
            }
        }
        else
        {
            Debug.Log($"Sector {currentPlayerSector.Name} is not yet cleared.");
        }
    }

    private void SimulateCombat()
    {

        foreach (var soldier in new List<ISoldier>(soldiers))
        {
            soldier.PerformAction(enemies);
        }

        foreach (var enemy in new List<Enemy>(enemies))
        {
            if (enemy != null) // Safety check before performing actions
            {
                enemy.PerformAction(soldiers);
            }
        }

        // Cleanup: Remove any destroyed enemies (null references) from the list
        soldiers.RemoveAll(s => s == null);
        enemies.RemoveAll(e => e == null);
    }

    protected void PurchaseSoldier(string type)
    {
        var spawnPosition = currentPlayerSector.GetBottomPosition();
        manpowerManager.PurchaseSoldier(type, spawnPosition, soldiers);
        UpdateManpowerUI();
    }

    private void MoveSoldiersToSector(Sector targetSector)
    {
        if (targetSector == null)
        {
            Debug.LogError("Target sector is null. Cannot move soldiers.");
            return;
        }

        Vector3 targetPosition = targetSector.GetBottomPosition();

        // Review the soldiers and update their positions
        for (int i = soldiers.Count - 1; i >= 0; i--)
        {
            var soldier = soldiers[i];

            // Check if soldier is null or destroyed
            if (soldier == null || (soldier as MonoBehaviour) == null)
            {
                soldiers.RemoveAt(i); // Remove destroyed soldiers from the list
                continue;
            }

            // Update position for valid soldiers
            soldier.Position = targetPosition;
            Debug.Log($"Moved soldier to sector: {targetSector.Name}");
        }
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

    /// <summary>
    /// Saving and Loading
    /// </summary>

    public void SaveGame()
    {
        saveHandler?.Save(this, timeManager, manpowerManager, moraleManager);
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGame()
    {
        var state = saveHandler?.Load();
        if (state != null) RestoreGameState(state);
    }

    private void RestoreGameState(GameState state)
    {
        // Restore basic game state
        currentDay = state.CurrentDay;
        timeManager.SetCurrentTime(state.CurrentTime);
        manpowerManager.SetManpower(state.Manpower);
        moraleManager.SetSummary(state.MoraleSummary);

        // Reinitialize sectors
        sectors.Clear();
        foreach (var sectorState in state.Sectors)
        {
            sectors.Add(new Sector(sectorState.Name)
            {
                Controlled = sectorState.Controlled,
                Owner = sectorState.Owner,
                Position = new Vector3(0, sectors.Count * 10, 0) // Adjust position logic as needed
            });
        }

        // Set current player sector
        currentPlayerSector = sectors.Find(s => s.Controlled); // Find the sector the player controls

        // Respawn enemies for the loaded day
        SpawnEnemiesAtDayStart(10); // Adjust amount as needed

        Debug.Log("Game state restored successfully.");
    }

    /// <summary>
    /// Utility Methods
    /// </summary>

    private void UpdateManpowerUI()
    {
        manpowerText.text = $"Manpower: {manpowerManager.GetCurrentManpower()}";
    }

    private void UpdateLighting()
    {
        timeManager.UpdateLighting();
    }

    private IEnumerator ShowDayEndPopup()
    {
        dayEndText.text = $"Day {currentDay} has ended.";
        dayEndPopup.SetActive(true);
        yield return new WaitForSeconds(5f);
        dayEndPopup.SetActive(false);
    }

    private bool AreAllEnemiesDefeatedInSector(Sector sector)
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && Vector3.Distance(enemy.transform.position, sector.Position) < 10f) // Adjust range as needed
            {
                return false;
            }
        }
        return true;
    }

    private Sector GetClosestEnemySector()
    {
        foreach (var sector in sectors)
        {
            // Find the first unclaimed sector that is not owned by the player
            if (!sector.Controlled && sector.Owner != 1)
            {
                return sector;
            }
        }

        return null; // No valid sector found
    }

    /// <summary>
    /// State Retrieval Methods
    /// </summary>


    public int GetCurrentDay()
    {
        return currentDay;
    }

    public List<SectorState> GetSectorsState()
    {
        List<SectorState> states = new List<SectorState>();
        foreach (Sector sector in sectors)
        {
            states.Add(new SectorState
            {
                Name = sector.Name,
                Controlled = sector.Controlled,
                Owner = sector.Owner
            });
        }
        return states;
    }

    public List<SoldierState> GetSoldiersState()
    {
        List<SoldierState> states = new List<SoldierState>();


        for (int i = soldiers.Count - 1; i >= 0; i--)
        {
            ISoldier soldier = soldiers[i];

            // Go through soldiers and process only valid ones
            if (soldier == null || (soldier as MonoBehaviour) == null)
            {
                soldiers.RemoveAt(i);
                continue;
            }


            states.Add(new SoldierState
            {
                Type = soldier is IBasicSoldier ? "Basic" : "Advanced",
                Position = new float[] { soldier.Position.x, soldier.Position.y, soldier.Position.z },
                Health = soldier.Health,
                Morale = soldier.Morale
            });
        }

        return states;
    }

    public List<EnemyState> GetEnemiesState()
    {
        List<EnemyState> states = new List<EnemyState>();

        // Go through enemies and process only valid ones
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];

            if (enemy == null || (enemy as MonoBehaviour) == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            states.Add(new EnemyState
            {
                Position = new float[] { enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z },
                Health = enemy.Health
            });
        }

        return states;
    }

    public void RemoveSoldier(ISoldier soldier)
    {
        soldiers.Remove(soldier);
        Debug.Log($"Soldier removed: {soldier.GetType().Name}");
    }

    /// <summary>
    /// Initialization Helpers
    /// </summary>

    private void InitializeSectors(int sectorHeight)
    {
        string[] sectorNames = { "A", "B", "C", "D", "E", "F", "G" };

        for (int i = 0; i < sectorNames.Length; i++)
        {
            sectors.Add(new Sector(sectorNames[i])
            {
                Controlled = i == 0,
                Owner = i == 0 ? 1 : 2,
                Position = new Vector3(0, i * sectorHeight, 0)
            });
        }
    }

    protected void SpawnEnemiesAtDayStart(int amount)
    {

        if (currentPlayerSector == null)
        {
            Debug.LogError("currentPlayerSector is null! Cannot spawn enemies.");
            return;
        }

        Vector3 centerPosition = currentPlayerSector.GetTopPosition();
        float startX = centerPosition.x - (amount / 2f) * 1.5f; // Start from the leftmost position

        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPosition = new Vector3(startX + i * 1.5f, centerPosition.y, -1);
            var enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, spawnParent).GetComponent<Enemy>();
            if (enemy != null)
            {
                enemies.Add(enemy);
            }
        }

        Debug.Log($"{amount} enemies spawned in sector: {currentPlayerSector.Name}");
    }

}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class ManpowerManager : MonoBehaviour
{
    private int manpower;
    private int supportDays;

    private MoraleManager moraleManager;
    private GameManager gameManager;

    public GameObject basicSoldierPrefab;
    public GameObject advancedSoldierPrefab;
    public Transform spawnParent;

    public ManpowerManager(int initialManpower, int supportDaysLimit, GameObject basicPrefab, GameObject advancedPrefab, Transform parent)
    {
        manpower = initialManpower;
        supportDays = supportDaysLimit;
        basicSoldierPrefab = basicPrefab;
        advancedSoldierPrefab = advancedPrefab;
        spawnParent = parent;
    }

    private void Awake()
    {
        manpower = 10000; // Set initial manpower here
        Debug.Log($"Manpower initialized to {manpower} in Awake.");
    }

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = Object.FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found! Ensure it is present in the scene and properly assigned.");
            }
        }
        if (manpower == 0)
        {
            manpower = 10000;
        }
    }

    public int GetCurrentManpower() => manpower;

    public void ReduceManpower(int amount)
    {
        manpower = Mathf.Max(0, manpower - amount);
    }

    public bool CanAfford(int cost)
    {
        return manpower >= cost;
    }

    public void PurchaseSoldier(string type, Vector3 spawnPosition, List<ISoldier> soldierList)
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager is not assigned in ManpowerManager!");
            return;
        }

        // Proceed with soldier purchase logic...
        int cost = type == "Basic" ? 10 : 50;
        GameObject soldierPrefab = type == "Basic" ? basicSoldierPrefab : advancedSoldierPrefab;

        if (CanAfford(cost))
        {
            ReduceManpower(cost);

            Vector3 adjustedPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            GameObject newSoldierObject = Instantiate(soldierPrefab, adjustedPosition, Quaternion.identity, spawnParent);
            ISoldier newSoldier = newSoldierObject.GetComponent<ISoldier>();

            if (newSoldier != null)
            {
                if (newSoldier is AdvancedSoldier advancedSoldier)
                {
                    advancedSoldier.Initialize(gameManager); // Inject GameManager
                }

                soldierList.Add(newSoldier);
                Debug.Log($"{type} soldier purchased and spawned at {spawnPosition}.");
            }
            else
            {
                Debug.LogError("Failed to spawn soldier: Component missing.");
            }
        }
        else
        {
            Debug.Log("Not enough manpower to purchase this soldier.");
        }
    }

    public void CalculateManpower(int moraleSummary)
    {
        if (supportDays > 0)
        {
            int manpowerIncrease = Mathf.Clamp(moraleSummary / 10, 0, 10) * 5;
            manpower += manpowerIncrease;
            Debug.Log($"Manpower increased by {manpowerIncrease}. Current manpower: {manpower}");
        }
        else
        {
            Debug.Log("No manpower increase. Support days exhausted.");
        }

    }

    public void SetManpower(int value)
    {
        manpower = value;
        Debug.Log($"Manpower set to {manpower}.");
    }

}
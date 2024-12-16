using System;
using System.Collections.Generic;
using UnityEngine;

// Manages the player's manpower, soldier purchasing, and related calculations.
public class ManpowerManager : MonoBehaviour
{
    // Core Variables
    protected int manpower;
    protected int supportDays;

    // References
    protected GameManager gameManager;
    public GameObject basicSoldierPrefab;
    public GameObject advancedSoldierPrefab;
    public Transform spawnParent;

    protected void Awake()
    {
        manpower = 10000; // Default initial manpower
    }

    protected void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
                Debug.LogError("GameManager not found! Ensure it is present in the scene.");
        }
    }

    // Gets the current manpower amount.
    public int GetCurrentManpower() => manpower;

    // Purchases a soldier and spawns it in the specified position.
    public void PurchaseSoldier(string type, Vector3 spawnPosition, List<ISoldier> soldierList)
    {
        int cost = (type == "Basic") ? 10 : 50;
        GameObject soldierPrefab = (type == "Basic") ? basicSoldierPrefab : advancedSoldierPrefab;

        if (CanAfford(cost))
        {
            ReduceManpower(cost);

            Vector3 adjustedPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            var soldierObject = Instantiate(soldierPrefab, adjustedPosition, Quaternion.identity, spawnParent);
            var soldier = soldierObject.GetComponent<ISoldier>();

            if (soldier != null)
            {
                if (soldier is AdvancedSoldier advancedSoldier)
                {
                    advancedSoldier.Initialize(gameManager); // Inject GameManager reference
                }

                soldierList.Add(soldier);
                Debug.Log($"{type} soldier purchased and spawned at {spawnPosition}.");
            }
            else
            {
                Debug.LogError("Failed to spawn soldier: Missing ISoldier component.");
            }
        }
        else
        {
            Debug.Log("Not enough manpower to purchase this soldier.");
        }
    }

    // Calculates daily manpower changes based on morale.
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

    // Checks if the player has enough manpower for a purchase.
    public bool CanAfford(int cost) => manpower >= cost;

    // Reduces manpower by a specified amount.
    public void ReduceManpower(int amount)
    {
        manpower = Mathf.Max(0, manpower - amount);
    }

    // Sets the manpower to a specific value (used during loading or debugging).
    public void SetManpower(int value)
    {
        manpower = value;
        Debug.Log($"Manpower set to {manpower}.");
    }
}
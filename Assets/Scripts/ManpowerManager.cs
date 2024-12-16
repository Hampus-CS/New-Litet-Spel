using System.Collections.Generic;
using UnityEngine;

public class ManpowerManager : MonoBehaviour
{
    private int manpower;
    private int supportDays;

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

    private void Start()
    {
        manpower = 100;
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
        int cost = type == "Basic" ? 10 : 20;
        GameObject soldierPrefab = type == "Basic" ? basicSoldierPrefab : advancedSoldierPrefab;

        if (CanAfford(cost))
        {
            ReduceManpower(cost);

            // Spawn soldier
            Vector3 adjustedPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            GameObject newSoldierObject = Instantiate(soldierPrefab, adjustedPosition, Quaternion.identity, spawnParent);
            ISoldier newSoldier = (ISoldier)newSoldierObject.GetComponent(typeof(ISoldier));
            soldierList.Add(newSoldier);

            Debug.Log($"{type} soldier purchased and spawned at {spawnPosition}.");
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
            manpower += Mathf.Clamp(moraleSummary / 10, 0, 10) * 5;
        }
    }
}
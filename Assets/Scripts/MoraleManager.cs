using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MoraleManager : MonoBehaviour
{
    private int moraleSummary;

    public void AddSoldierMorale(ISoldier soldier)
    {
        moraleSummary += soldier.Morale;
        Debug.Log($"Added soldier morale. Current morale summary: {moraleSummary}");
    }

    public void UpdateSoldierMorale(ISoldier soldier, int change)
    {
        soldier.Morale = Mathf.Clamp(soldier.Morale + change, 0, 10);

        if (soldier.Morale < 3)
        {
            Debug.Log("Soldier defects due to low morale.");
            Destroy(soldier as MonoBehaviour); // Destroy soldier GameObject
        }
        else
        {
            Debug.Log($"Updated soldier morale to: {soldier.Morale}");
        }
    }

    public int GetAverageMorale(List<ISoldier> soldiers)
    {
        if (soldiers.Count == 0) return 0;
        int totalMorale = 0;
        foreach (var soldier in soldiers)
        {
            totalMorale += soldier.Morale;
        }
        return totalMorale / soldiers.Count;
    }

    public void ResetSummary()
    {
        moraleSummary = 0;
    }

    public int GetSummary() => moraleSummary;
}
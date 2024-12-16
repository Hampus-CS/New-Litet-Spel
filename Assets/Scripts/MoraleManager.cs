using System.Collections.Generic;
using UnityEngine;

// Manages the morale of soldiers and calculates morale-related impacts on gameplay.
public class MoraleManager : MonoBehaviour
{
    // Core Variable
    protected int moraleSummary;

    // Updates a soldier's morale and handles defecting logic if morale becomes too low.
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

    // Gets the average morale of all soldiers in the list.
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

    // Adds a soldier's morale to the summary.
    public void AddSoldierMorale(ISoldier soldier)
    {
        moraleSummary += soldier.Morale;
        Debug.Log($"Added soldier morale. Current morale summary: {moraleSummary}");
    }

    // Resets the morale summary at the start of a new day.
    public void ResetSummary()
    {
        moraleSummary = 0;
    }

    // Gets the total morale summary.
    public int GetSummary() => moraleSummary;

    // Sets the morale summary (used during loading or debugging).
    public void SetSummary(int value)
    {
        moraleSummary = value;
        Debug.Log($"Morale summary set to {moraleSummary}.");
    }
}
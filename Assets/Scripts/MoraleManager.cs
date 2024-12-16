using UnityEngine;

public class MoraleManager : MonoBehaviour
{
    private int moraleSummary;

    public void AddSoldierMorale(ISoldier soldier)
    {
        moraleSummary += soldier.Morale;
    }

    public void ResetSummary()
    {
        moraleSummary = 0;
    }

    public int GetSummary() => moraleSummary;
}
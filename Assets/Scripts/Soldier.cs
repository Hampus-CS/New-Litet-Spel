using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

// Interface for a soldier
public interface ISoldier
{
    int Cost { get; }
    int Morale { get; set; }
    Vector3 Position { get; set; }
    int Health { get; set; } // Add health
    void PerformAction(List<Enemy> enemies);
    void TakeDamage(int damage); // Add damage handling
}

// Interface for basic soldier
public interface IBasicSoldier : ISoldier
{
    void PerformBasicAction(List<Enemy> enemies);
}

// Interface for advanced soldier
public interface IAdvancedSoldier : ISoldier
{
    void PerformAdvancedAction(List<Enemy> enemies);
}

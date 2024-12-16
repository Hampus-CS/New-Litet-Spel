using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    
}

// Interface for general soldier functionality.
public interface ISoldier
{
    int Cost { get; }
    int Morale { get; set; }
    Vector3 Position { get; set; }
    int Health { get; set; }

    void PerformAction(List<Enemy> enemies); // Executes soldier's action on enemies.
    void TakeDamage(int damage);            // Handles receiving damage.
}

// Interface for basic soldier functionality.
public interface IBasicSoldier : ISoldier
{
    void PerformBasicAction(List<Enemy> enemies); // Executes a specific action for basic soldiers.
}

// Interface for advanced soldier functionality.
public interface IAdvancedSoldier : ISoldier
{
    void PerformAdvancedAction(List<Enemy> enemies); // Executes a specific action for advanced soldiers.
}
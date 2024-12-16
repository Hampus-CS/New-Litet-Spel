using System.Collections.Generic;
using UnityEngine;

// Represents an advanced soldier with higher stats and unique actions.
public class AdvancedSoldier : MonoBehaviour, IAdvancedSoldier
{

    [Header("Soldier Properties")]
    public int Cost => 50;
    public int Morale { get; set; } = 8;
    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public int Health { get; set; } = 200; // Higher health for advanced soldiers

    // References
    private GameManager gameManager;

    // Initializes the soldier with a reference to the GameManager.
    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    // Executes the soldier's action, attacking the nearest enemy.
    public void PerformAction(List<Enemy> enemies)
    {
        if (this == null) return; // Ensure the soldier is not destroyed

        if (Morale < 3)
        {
            Debug.Log("Advanced soldier defects due to low morale.");
            gameManager.RemoveSoldier(this);
            Destroy(gameObject);
            return;
        }

        Enemy nearestEnemy = FindNearestEnemy(enemies);
        if (nearestEnemy != null)
        {
            int damage = Morale > 8 ? 30 : 20; // Higher morale deals more damage
            nearestEnemy.TakeDamage(damage);
            Debug.Log($"Advanced soldier attacks the nearest enemy with morale: {Morale}");
        }
    }

    // Handles receiving damage, reducing health and morale.
    public void TakeDamage(int damage)
    {
        Health -= damage;
        Morale = Mathf.Max(0, Morale - 2); // Advanced soldiers lose more morale when hit

        if (Health <= 0)
        {
            Debug.Log("Advanced soldier defeated.");
            Destroy(gameObject);
        }
    }

    // Executes a specialized action for advanced soldiers.
    public void PerformAdvancedAction(List<Enemy> enemies)
    {
        Enemy nearestEnemy = FindNearestEnemy(enemies);
        if (nearestEnemy != null)
        {
            nearestEnemy.TakeDamage(20); // Fixed damage for advanced action
            Debug.Log("Advanced soldier attacks the nearest enemy.");
        }
    }

    // Finds the nearest enemy from a list of enemies.
    private Enemy FindNearestEnemy(List<Enemy> enemies)
    {
        Enemy nearestEnemy = null;
        float shortestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue; // Skip null or destroyed enemies

            float distance = Vector3.Distance(Position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }
        return nearestEnemy;
    }
}
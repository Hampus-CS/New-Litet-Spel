using System.Collections.Generic;
using UnityEngine;

// Represents a basic soldier with standard stats and actions.
public class BasicSoldier : MonoBehaviour, IBasicSoldier
{

    [Header("Soldier Properties")]
    public int Cost => 10;
    public int Morale { get; set; } = 5;
    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public int Health { get; set; } = 100; // Default health for basic soldiers

    // Executes the soldier's action, attacking the nearest enemy.
    public void PerformAction(List<Enemy> enemies)
    {
        if (this == null) return; // Ensure the soldier is not destroyed

        if (Morale < 3)
        {
            Debug.Log("Basic soldier defects due to low morale.");
            Destroy(gameObject);
            return;
        }

        Enemy nearestEnemy = FindNearestEnemy(enemies);
        if (nearestEnemy != null)
        {
            int damage = Morale > 8 ? 15 : 10; // Higher morale deals more damage
            nearestEnemy.TakeDamage(damage);
            Debug.Log($"Basic soldier attacks the nearest enemy with morale: {Morale}");
        }
    }

    // Handles receiving damage, reducing health and morale.
    public void TakeDamage(int damage)
    {
        Health -= damage;
        Morale = Mathf.Max(0, Morale - 1); // Basic soldiers lose 1 morale when hit

        if (Health <= 0)
        {
            Debug.Log("Basic soldier defeated.");
            Destroy(gameObject);
        }
    }

    // Executes a specialized action for basic soldiers.
    public void PerformBasicAction(List<Enemy> enemies)
    {
        Enemy nearestEnemy = FindNearestEnemy(enemies);
        if (nearestEnemy != null)
        {
            nearestEnemy.TakeDamage(10); // Fixed damage for basic action
            Debug.Log("Basic soldier attacks the nearest enemy.");
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
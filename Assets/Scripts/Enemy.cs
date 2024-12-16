using System.Collections.Generic;
using UnityEngine;

// Represents an enemy that interacts with soldiers and the game world.
public class Enemy : MonoBehaviour
{
    // Enemy Properties
    public int Health { get; set; } = 100;

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Die();
        }
    }

    // Executes the enemy's action, targeting the nearest soldier.
    public void PerformAction(List<ISoldier> soldiers)
    {
        if (this == null) return; // Ensure the enemy is not destroyed

        ISoldier nearestSoldier = FindNearestSoldier(soldiers);
        if (nearestSoldier != null && nearestSoldier.Health > 0)
        {
            nearestSoldier.TakeDamage(10); // Deal damage
            nearestSoldier.Morale = Mathf.Max(0, nearestSoldier.Morale - 1); // Reduce morale
            Debug.Log("Enemy attacks the nearest soldier.");
        }
    }

    // Finds the nearest soldier from a list of soldiers.
    private ISoldier FindNearestSoldier(List<ISoldier> soldiers)
    {
        ISoldier nearestSoldier = null;
        float shortestDistance = float.MaxValue;

        foreach (var soldier in soldiers)
        {
            if (soldier == null || (soldier as MonoBehaviour) == null) continue;

            float distance = Vector3.Distance(transform.position, soldier.Position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestSoldier = soldier;
            }
        }

        return nearestSoldier;
    }

    private void Die()
    {
        Debug.Log("Enemy defeated.");
        Destroy(gameObject);
    }
}
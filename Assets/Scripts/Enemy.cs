using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Enemy : MonoBehaviour
{
    public int Health { get; set; } = 100; // Default health

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy defeated.");
        Destroy(gameObject); // Destroy the enemy GameObject
    }

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

    private ISoldier FindNearestSoldier(List<ISoldier> soldiers)
    {
        ISoldier nearestSoldier = null;
        float shortestDistance = float.MaxValue;

        foreach (var soldier in soldiers)
        {
            if (soldier == null) continue; // Skip destroyed soldiers

            float distance = Vector3.Distance(transform.position, soldier.Position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestSoldier = soldier;
            }
        }
        return nearestSoldier;
    }
}

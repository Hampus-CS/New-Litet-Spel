using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class BasicSoldier : MonoBehaviour, IBasicSoldier
{
    public int Cost => 10;
    public int Morale { get; set; } = 5;
    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public int Health { get; set; } = 100; // Default health

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
            int damage = Morale > 8 ? 15 : 10; // Higher damage for high morale
            nearestEnemy.TakeDamage(damage);
            Debug.Log($"Basic soldier attacks the nearest enemy with morale: {Morale}");
        }
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        Morale = Mathf.Max(0, Morale - 1); // Reduce morale when taking damage
        if (Health <= 0)
        {
            Debug.Log("Basic soldier defeated.");
            Destroy(gameObject);
        }
    }

    public void PerformBasicAction(List<Enemy> enemies)
    {
        Enemy nearestEnemy = FindNearestEnemy(enemies);
        if (nearestEnemy != null)
        {
            nearestEnemy.TakeDamage(10);
            Debug.Log("Basic soldier attacks the nearest enemy.");
        }
    }

    private Enemy FindNearestEnemy(List<Enemy> enemies)
    {
        Enemy nearestEnemy = null;
        float shortestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
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
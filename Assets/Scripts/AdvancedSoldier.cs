using System.Collections.Generic;
using UnityEngine;

public class AdvancedSoldier : MonoBehaviour, IAdvancedSoldier
{
    public int Cost => 20;
    public int Morale { get; set; } = 8;
    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public int Health { get; set; } = 150; // Higher health for advanced soldier

    public void PerformAction(List<Enemy> enemies)
    {
        PerformAdvancedAction(enemies);
    }

    public void PerformAdvancedAction(List<Enemy> enemies)
    {
        Enemy nearestEnemy = FindNearestEnemy(enemies);
        if (nearestEnemy != null)
        {
            nearestEnemy.TakeDamage(20);
            Debug.Log("Advanced soldier attacks the nearest enemy.");
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

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Debug.Log("Advanced soldier defeated.");
            Destroy(gameObject);
        }
    }
}

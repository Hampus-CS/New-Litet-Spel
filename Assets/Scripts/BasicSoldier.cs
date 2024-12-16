using System.Collections.Generic;
using UnityEngine;

public class BasicSoldier : MonoBehaviour, IBasicSoldier
{
    public int Cost => 10;
    public int Morale { get; set; } = 5;
    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public int Health { get; set; } = 100; // Default health

    public void PerformAction(List<Enemy> enemies)
    {
        PerformBasicAction(enemies);
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

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Debug.Log("Basic soldier defeated.");
            Destroy(gameObject);
        }
    }
}
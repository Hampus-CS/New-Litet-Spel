using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AdvancedSoldier : MonoBehaviour, IAdvancedSoldier
{
    public int Cost => 50;
    public int Morale { get; set; } = 8;
    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public int Health { get; set; } = 200; // Higher health for advanced soldier

    private GameManager gameManager;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    public void PerformAction(List<Enemy> enemies)
    {

        if (this == null) return; // Ensure the soldier is not destroyed

        if (gameManager == null)
        {
            Debug.LogError("GameManager reference is null in AdvancedSoldier. Ensure it is properly initialized.");
            return;
        }

        if (Morale < 3)
        {
            Debug.Log("Advanced soldier defects due to low morale.");
            gameManager.RemoveSoldier(this); // Ensure soldier is removed from GameManager's list
            Destroy(gameObject);
            return;
        }

        Enemy nearestEnemy = FindNearestEnemy(enemies);
        if (nearestEnemy != null)
        {
            int damage = Morale > 8 ? 30 : 20; // Higher damage for high morale
            nearestEnemy.TakeDamage(damage);
            Debug.Log($"Advanced soldier attacks the nearest enemy with morale: {Morale}");
        }
    }

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

}

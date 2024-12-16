using UnityEngine;

public class Sector
{
    public string Name { get; private set; }
    public bool Controlled { get; set; }
    public int Owner { get; set; } // 0 = Neutral, 1 = Player, 2 = Enemy
    public BoundsInt TileBounds { get; set; } // Optional for tile handling
    public Vector3 Position { get; set; } // Added position for sectors

    public Sector(string name)
    {
        Name = name;
        Controlled = false;
        Owner = 0;
        Position = Vector3.zero; // Default position
    }

    public Vector3 GetTopPosition()
    {
        return new Vector3(Position.x, Position.y + 5, -1); // Adjust +5 for the top offset
    }

    public Vector3 GetBottomPosition()
    {
        return new Vector3(Position.x, Position.y - 5, -1); // Always set Z to -1
    }
}
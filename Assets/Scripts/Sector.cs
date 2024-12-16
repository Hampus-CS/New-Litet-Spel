using UnityEngine;

// Represents a single sector on the map, including ownership and positional data.
public class Sector
{
    // Sector Properties
    public string Name { get; private set; }
    public bool Controlled { get; set; }
    public int Owner { get; set; } // 0 = Neutral, 1 = Player, 2 = Enemy
    public Vector3 Position { get; set; } // Center position of the sector

    // Constructor
    public Sector(string name)
    {
        Name = name;
        Controlled = false;
        Owner = 0; // Neutral by default
        Position = Vector3.zero;
    }

    // Gets the top position of the sector (used for spawning enemies).
    public Vector3 GetTopPosition()
    {
        return new Vector3(Position.x, Position.y + 5, -1); // Adjusts Y-axis for the top
    }

    // Gets the bottom position of the sector (used for spawning player units).
    public Vector3 GetBottomPosition()
    {
        return new Vector3(Position.x, Position.y - 5, -1); // Adjusts Y-axis for the bottom
    }
}
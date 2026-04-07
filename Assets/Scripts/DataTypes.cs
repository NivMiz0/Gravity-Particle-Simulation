using UnityEngine;
[System.Serializable]
public struct Planet
{
    public Vector2 position;
    public float radius;
    public static int GetSize()
    {
        return sizeof(float)*3;
    }
    public Planet(Vector2 position, float radius)
    {
        this.position = position;
        this.radius = radius;
    }
}

[System.Serializable]
public struct Particle
{
    public Vector2 position;
    public Vector2 velocity;
    public Particle(Vector2 position, Vector2 velocity)
    {
        this.position = position;
        this.velocity = velocity;
    }
    public static int GetSize()
    {
        return sizeof(float)*4;
    }
    public override string ToString()
    {
        return $"position: {position} velocity: {velocity}";
    }
    
}
using UnityEngine;
[System.Serializable]
public struct Planet
{
    public Vector2 position;
    public float radius;
    public float mass;
    public static int GetSize()
    {
        return sizeof(float)*4;
    }
    public Planet(Vector2 position, float radius, float mass)
    {
        this.position = position;
        this.radius = radius;
        this.mass = mass;
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
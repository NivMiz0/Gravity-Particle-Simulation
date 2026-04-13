using UnityEngine;

public static class HelperFuncs
{
    public static Vector2 WorldToUV(Vector2 world, int width, int height)
    {
        return new Vector2(world.x*width/18 + width/2, world.y*height/10 + height/2);
    }
}

[System.Serializable]
public struct Planet
{
    public Vector2 position;
    public float radius;
    public float mass;
    public Color color;
    public static int GetSize()
    {
        return sizeof(float)*8;
    }
    public Planet(Vector2 position, float radius, float mass, Color color)
    {
        this.position = position;
        this.radius = radius;
        this.mass = mass;
        this.color = color;
    }
}

[System.Serializable]
public struct Particle
{
    public Vector2 position;
    public Vector2 velocity;
    public float radius;
    public Particle(Vector2 position, Vector2 velocity, float radius)
    {
        this.position = position;
        this.velocity = velocity;
        this.radius = radius;
    }
    public static int GetSize()
    {
        return sizeof(float)*5;
    }
    public override string ToString()
    {
        return $"position: {position} velocity: {velocity}";
    }
    
}
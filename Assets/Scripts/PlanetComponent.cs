using UnityEditor;
using UnityEngine;

class PlanetComponent : MonoBehaviour
{
    [SerializeField] float radius;
    [SerializeField] float mass;
    [SerializeField] Color color;
    public Planet GetData(int width, int height)
    {
        return new Planet(HelperFuncs.WorldToPixelCoords(transform.position, width, height), radius, mass, color);
    }
}
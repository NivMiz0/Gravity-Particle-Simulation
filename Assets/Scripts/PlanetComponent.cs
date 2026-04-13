using UnityEditor;
using UnityEngine;

class PlanetComponent : MonoBehaviour
{
    [SerializeField] float radius;
    [SerializeField] float mass;
    [SerializeField] Color color;
    public Planet GetData(int width, int height)
    {
        return new Planet(HelperFuncs.WorldToUV(transform.position, width, height), radius, mass, color);
    }

    // void OnDrawGizmos()
    // {
    //     Handles.color = Color.red;
    //     Handles.DrawSolidDisc((Vector2)transform.position,Vector3.forward, radius);
    // }
    
}
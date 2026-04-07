using UnityEditor;
using UnityEngine;

class PlanetComponent : MonoBehaviour
{
    [SerializeField] float radius;
    public Planet GetData()
    {
        return new Planet(transform.position, radius);
    }

    void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawSolidDisc((Vector2)transform.position,Vector3.forward, radius);
    }
    
}
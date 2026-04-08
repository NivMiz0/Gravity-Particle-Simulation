using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RenderSimulation : MonoBehaviour
{
    [SerializeField] RawImage screenUI;
    [SerializeField] ComputeShader compute;
    Camera cam;
    RenderTexture screenTexture;
    [SerializeField] Color backgroundColor;
    [SerializeField] int width;
    int height = 0;
    
    ComputeBuffer planetsBuffer;
    ComputeBuffer particlesBuffer;
    List<Particle> particlesList;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        height = Mathf.RoundToInt(width * 1/cam.aspect);
        particlesList = new List<Particle>();
    }

    void FixedUpdate()
    {
        StepSimulation();
    }

    
    void StepSimulation()
    {
        if(screenTexture == null)
        {
            Init();
        }
           
        SendPlanets();
        
        int numParticles;     
        particlesList.Add(new Particle(new Vector2(0, 0), new Vector2(0f, -1f)));
        SendParticles(out numParticles);
        
        compute.Dispatch(0,Mathf.CeilToInt(numParticles/4f), 1, 1); //Simulation Step
        compute.Dispatch(1, Mathf.CeilToInt(width/8f), Mathf.CeilToInt(height/8f), 1); //Render to Screen
        
        Particle[] ps = new Particle[numParticles];
        particlesBuffer.GetData(ps); // TODO:This is bad and slow. Track particles fully on GPU?
        particlesList = ps.ToList();
    }
    
    void SendPlanets()
    {
        PlanetComponent[] planets = FindObjectsByType<PlanetComponent>();
        Planet[] planetsData = planets.Select(p => p.GetData()).ToArray();
        
        if(planetsBuffer != null) { planetsBuffer.Release(); planetsBuffer = null; }
        planetsBuffer = new ComputeBuffer(planets.Length, Planet.GetSize());
        planetsBuffer.SetData(planetsData);
        
        compute.SetBuffer(0, "Planets", planetsBuffer);
        compute.SetBuffer(1, "Planets", planetsBuffer);
        
        compute.SetInt("NumPlanets", planetsData.Length);
    }
    
    void SendParticles(out int numParticles)
    {
        Particle[] particleData = particlesList.ToArray();
        if(particleData.Length == 0) //Zero length buffer guard
        {
            particleData = particleData.Append(new Particle(Vector2.zero, Vector2.zero)).ToArray();
        }
        numParticles = particleData.Length;
        
        if(particlesBuffer != null) { particlesBuffer.Release(); particlesBuffer = null; }
        particlesBuffer = new ComputeBuffer(particleData.Length, Particle.GetSize());
        particlesBuffer.SetData(particleData);
        
        compute.SetBuffer(0, "Particles", particlesBuffer);
        compute.SetBuffer(1, "Particles", particlesBuffer);
        
        compute.SetInt("NumParticles", particleData.Length);
    }
    
    void Init()
    {
        screenTexture = new RenderTexture(width, height, 1, RenderTextureFormat.ARGBHalf)
        {   
            filterMode = FilterMode.Point,
            enableRandomWrite = true
        };
        screenTexture.Create();
        
        compute.SetTexture(1, "Result", screenTexture);
                
        compute.SetVector("BGColor", backgroundColor);
        compute.SetInt("WIDTH", width);
        compute.SetInt("HEIGHT", height);
                
        screenUI.texture = screenTexture;
    }

    void OnDisable()
    {
        planetsBuffer.Release();
        particlesBuffer.Release();
    }
}
 
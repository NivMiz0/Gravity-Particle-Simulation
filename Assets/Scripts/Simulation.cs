using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Simulation : MonoBehaviour
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
    
    bool doSimulation = false;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        height = Mathf.RoundToInt(width * 1/cam.aspect);
        particlesList = new List<Particle>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            FetchParticles();
            print(particlesList.Count);
            doSimulation = !doSimulation;
        }
        if(doSimulation) return;
        if(Input.GetMouseButton(0))
        {
            Vector2 p = cam.ScreenToWorldPoint(Input.mousePosition);
            for (int i = 0; i < 100; i++)
            {
                SpawnParticles(HelperFuncs.WorldToUV(p + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)), width, height));   
            }
        }
        
    }

    void FixedUpdate()
    { 
        if(screenTexture == null)
        {
            Init();
        }
        RenderBackground();
        RenderParticles();
        if(!doSimulation) return;
        StepSimulation();
    }
    
    void RenderBackground()
    {
        SendPlanets();
        compute.Dispatch(1, Mathf.CeilToInt(width/8f), Mathf.CeilToInt(height/8f), 1);
    }
    void RenderParticles()
    {
        compute.Dispatch(2, Mathf.CeilToInt(particlesList.Count/64f), 1, 1); //Render to Screen
    }
    
    void StepSimulation()
    {
        // SpawnParticles(HelperFuncs.WorldToUV(Vector2.zero, width, height));
        compute.Dispatch(0,Mathf.CeilToInt(particlesList.Count/4f), 1, 1); //Simulation Step
        // FetchParticles(); 
    }
    
    void SendPlanets()
    {
        PlanetComponent[] planets = FindObjectsByType<PlanetComponent>();
        Planet[] planetsData = planets.Select(p => p.GetData(width, height)).ToArray();
        
        if(planetsBuffer != null) { planetsBuffer.Release(); planetsBuffer = null; }
        planetsBuffer = new ComputeBuffer(planets.Length, Planet.GetSize());
        planetsBuffer.SetData(planetsData);
        
        compute.SetBuffer(0, "Planets", planetsBuffer);
        compute.SetBuffer(1, "Planets", planetsBuffer);
        compute.SetBuffer(2, "Planets", planetsBuffer);
        
        compute.SetInt("NumPlanets", planetsData.Length);
    }
    
    void SpawnParticles(Vector2 pos)
    {
        particlesList.Add(new Particle(pos, new Vector2(0f, -1)));
        SendParticles(particlesList.ToArray());
    }
    
    void SendParticles(Particle[] toSend)
    {
        Particle[] particleData = toSend;
        
        if(particlesBuffer != null) { particlesBuffer.Release(); particlesBuffer = null; }
        particlesBuffer = new ComputeBuffer(particleData.Length, Particle.GetSize());
        particlesBuffer.SetData(particleData);
        
        compute.SetBuffer(0, "Particles", particlesBuffer);
        compute.SetBuffer(2, "Particles", particlesBuffer);
        
        compute.SetInt("NumParticles", particleData.Length);
    }
    
    void FetchParticles()
    {
        Particle[] ps = new Particle[particlesList.Count];
        particlesBuffer.GetData(ps);
        particlesList = ps.ToList();
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
        compute.SetTexture(2, "Result", screenTexture);
                
        compute.SetVector("BGColor", backgroundColor);
        compute.SetInt("WIDTH", width); 
        compute.SetInt("HEIGHT", height);
        
        particlesList.Add(new Particle(Vector2.zero, Vector2.zero));
        SendParticles(particlesList.ToArray());
                
        screenUI.texture = screenTexture;
    }

    void OnDisable()
    {
        planetsBuffer.Release();
        particlesBuffer.Release();
    }
}
 
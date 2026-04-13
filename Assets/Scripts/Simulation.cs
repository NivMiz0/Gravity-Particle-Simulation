using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Simulation : MonoBehaviour
{
    [SerializeField] ComputeShader compute;
    [SerializeField] RawImage screenUI;
    Camera cam;
    RenderTexture screenTexture;
    [SerializeField] int width;
    int height = 0;
    ComputeBuffer planetsBuffer;
    ComputeBuffer particlesBuffer;
    List<Particle> particlesList;
    [SerializeField] float brushSize;
    [SerializeField] Color backgroundColor;
    [SerializeField] Gradient particleColorGrad;
    ComputeBuffer gradientBuffer;
    [SerializeField] float particleRadius;    
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
            SpawnParticles(p, 10000, brushSize/2);   
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
        compute.Dispatch(2, Mathf.Min(Mathf.CeilToInt(particlesList.Count/1024f), 65535), 1, 1); //Render to Screen
    }
    
    void StepSimulation()
    {
        compute.Dispatch(0, Mathf.Min(Mathf.CeilToInt(particlesList.Count/1024f), 65535), 1, 1); //Simulation Step
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
    
    void SpawnParticles(Vector2 pos, int num, float randomOffset)
    {
        for (int i = 0; i < num; i++)
        {
            Vector2 randomizedPos = HelperFuncs.WorldToUV(pos + new Vector2(Random.Range(-randomOffset, randomOffset), Random.Range(-randomOffset, randomOffset)), width, height);
            particlesList.Add(new Particle(randomizedPos, Vector2.down, particleRadius));
        }
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
        
        if(gradientBuffer != null) { gradientBuffer.Release(); gradientBuffer = null; }
        gradientBuffer = new ComputeBuffer(particleColorGrad.colorKeyCount, sizeof(float)*4);
        gradientBuffer.SetData(particleColorGrad.colorKeys.Select(c => c.color).ToArray());
        compute.SetBuffer(2, "ParticleColorGradient", gradientBuffer);
        compute.SetInt("NumParticleColors", particleColorGrad.colorKeyCount);
        
        particlesList.Add(new Particle(Vector2.zero, Vector2.zero, 0));
        SendParticles(particlesList.ToArray());
                
        screenUI.texture = screenTexture;
    }

    void OnDisable()
    {
        planetsBuffer?.Release();
        particlesBuffer?.Release();
        gradientBuffer?.Release();
    }
}
 
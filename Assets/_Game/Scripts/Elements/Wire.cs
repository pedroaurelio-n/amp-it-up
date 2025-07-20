using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [field: SerializeField] public LineRenderer LineRenderer { get; private set; }

    [SerializeField] WireMeshGenerator wireMeshGenerator;
    [SerializeField] Material[] materials;
    [SerializeField] ParticleSystem startBurst;
    [SerializeField] ParticleSystem endBurst;
    [SerializeField, ColorUsage(true, true)] Color poweredColor;
    
    public Vector3 StartPoint => new(LineRenderer.GetPosition(0).x, 0f, LineRenderer.GetPosition(0).z);

    public Vector3 EndPoint => new(
        LineRenderer.GetPosition(LineRenderer.positionCount - 1).x,
        0f,
        LineRenderer.GetPosition(LineRenderer.positionCount - 1).z
    );
    
    Vector3 StartParticlePoint => (StartPoint + new Vector3(
        LineRenderer.GetPosition(1).x,
        0f,
        LineRenderer.GetPosition(1).z
    )) * 0.5f;
    
    Vector3 EndParticlePoint => (EndPoint + new Vector3(
        LineRenderer.GetPosition(LineRenderer.positionCount - 2).x,
        0f,
        LineRenderer.GetPosition(LineRenderer.positionCount - 2).z
    )) * 0.5f;

    public int Price { get; set; }
    public bool IsPowered { get; private set; }

    GridGenerator _gridGenerator;

    public void Setup (GridGenerator generator)
    {
        _gridGenerator = generator;
        LevelManager.Instance.RegisterWire(this);
    }

    public void SetWireMesh ()
    {
        Vector3[] pos = new Vector3[LineRenderer.positionCount];
        LineRenderer.GetPositions(pos);
        wireMeshGenerator.GenerateMesh(pos);
        LineRenderer.enabled = false;
    }

    public void SetPoweredState (bool active)
    {
        bool oldState = IsPowered;
        IsPowered = active;
        LineRenderer.material = IsPowered ? materials[1] : materials[0];
        
        Material wireMat = wireMeshGenerator.MeshMaterial;
        wireMat.SetColor("_BaseColor", IsPowered ? poweredColor : Color.black);
        wireMat.SetFloat("_PulseIntensity", IsPowered ? 2f : 0f);
        wireMat.SetFloat("_WaveTime", IsPowered ? 3.2f : 0f);

        if (IsPowered && !oldState)
        {
            startBurst.transform.position = StartParticlePoint;
            startBurst.Play();
            
            endBurst.transform.position = EndParticlePoint;
            endBurst.Play();
        }
    }

    public GridTile GetStartTile() => _gridGenerator.GetTileByCenterPoint(StartPoint);
    public GridTile GetEndTile() => _gridGenerator.GetTileByCenterPoint(EndPoint);
}
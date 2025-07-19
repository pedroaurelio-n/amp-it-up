using UnityEngine;

public class Wire : MonoBehaviour
{
    [field: SerializeField] public LineRenderer LineRenderer { get; private set; }

    [SerializeField] Material[] materials;
    
    public Vector3 StartPoint => new(LineRenderer.GetPosition(0).x, 0f, LineRenderer.GetPosition(0).z);

    public Vector3 EndPoint => new(
        LineRenderer.GetPosition(LineRenderer.positionCount - 1).x,
        0f,
        LineRenderer.GetPosition(LineRenderer.positionCount - 1).z
    );

    public int Price { get; set; }
    public bool IsPowered { get; private set; }

    GridGenerator _gridGenerator;

    public void Setup (GridGenerator generator)
    {
        _gridGenerator = generator;
        LevelManager.Instance.RegisterWire(this);
    }

    public void SetPoweredState (bool active)
    {
        IsPowered = active;
        LineRenderer.material = IsPowered ? materials[1] : materials[0];
    }

    public GridTile GetStartTile() => _gridGenerator.GetTileByCenterPoint(StartPoint);
    public GridTile GetEndTile() => _gridGenerator.GetTileByCenterPoint(EndPoint);
}
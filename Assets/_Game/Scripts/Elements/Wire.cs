using UnityEngine;

public class Wire : MonoBehaviour
{
    [field: SerializeField] public LineRenderer LineRenderer { get; private set; }

    [SerializeField] Material[] materials;
    
    public Vector3 StartPoint => LineRenderer.GetPosition(0);
    public Vector3 EndPoint => LineRenderer.GetPosition(LineRenderer.positionCount - 1);

    public int Length => LineRenderer.positionCount - 1;
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
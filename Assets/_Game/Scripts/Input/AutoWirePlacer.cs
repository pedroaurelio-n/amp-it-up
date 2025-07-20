using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class AutoWirePlacer : MonoBehaviour
{
    [SerializeField] Wire wirePrefab;
    [SerializeField] Wire ghostWirePrefab;
    [SerializeField] LayerMask tileLayer;
    
    public bool IsPlacing { get; private set; }

    List<Vector3> _wirePoints = new();
    
    WirePlacer _wirePlacer;
    GridGenerator _gridGenerator;
    Vector3? _startPoint;
    HashSet<Vector3> _localVisitedTiles = new();
    bool _isActive = true;

    public void Setup (WirePlacer wirePlacer, GridGenerator gridGenerator)
    {
        _wirePlacer = wirePlacer;
        _gridGenerator = gridGenerator;
    }

    public void Reset ()
    {
        IsPlacing = false;
        _startPoint = null;
        ClearWirePoints();
        _localVisitedTiles.Clear();
    }
    
    public void ClearWirePoints() => _wirePoints.Clear();

    void Update ()
    {
        if (!_isActive)
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            if (TryRaycastTile(out Vector3? tileCenter))
            {
                if (tileCenter == null)
                {
                    LevelManager.Instance.TriggerOnWireImpossible();
                    return;
                }

                GridTile tile = _gridGenerator.GetTileByCenterPoint(tileCenter.Value);
                if (tile == null || tile.IsObstacle)
                {
                    ClearCurrentLine(_wirePlacer.CurrentWire);
                    LevelManager.Instance.TriggerOnWireImpossible();
                    return;
                }
                
                if (_startPoint == null)
                {
                    if (!tile.IsGenerator && !tile.IsPole)
                    {
                        LevelManager.Instance.TriggerOnWireImpossible();
                        return;
                    }

                    tile.TryGetComponent(out Generator generator);
                    tile.TryGetComponent(out Pole pole);
                    generator?.Click();
                    pole?.Click();
                    
                    _startPoint = tileCenter;

                    IsPlacing = true;
                    _wirePlacer.CurrentWire = Instantiate(wirePrefab, transform.position, Quaternion.identity, _wirePlacer.transform);
                    _wirePlacer.CurrentWire.LineRenderer.positionCount = 0;
                    return;
                }

                if (tile.IsObstacle || (!tile.IsStructure && !tile.IsPole && !tile.IsGenerator))
                {
                    ClearCurrentLine(_wirePlacer.CurrentWire);
                    LevelManager.Instance.TriggerOnWireImpossible();
                    return;
                }
                
                tile.TryGetComponent(out Generator generator2);
                tile.TryGetComponent(out Pole pole2);
                tile.TryGetComponent(out Structure structure2);
                generator2?.Click();
                pole2?.Click();
                structure2?.Click();
                
                Vector3 endPoint = tileCenter.Value;
                GenerateAutomaticPath(_wirePlacer.CurrentWire, _startPoint.Value, endPoint, true);
                _startPoint = null;
                _wirePlacer.CurrentWire = null;
                IsPlacing = false;
            }
            else
            {
                ClearCurrentLine(_wirePlacer.CurrentWire);
                LevelManager.Instance.TriggerOnWireImpossible();
            }
        }
        
        if (IsPlacing)
        {
            if (_wirePlacer.GhostWire == null)
                _wirePlacer.GhostWire = Instantiate(ghostWirePrefab, transform.position, Quaternion.identity, _wirePlacer.transform);
        
            if (TryRaycastTile(out Vector3? tileCenter))
            {
                if (tileCenter == null)
                    return;
                Vector3 endPoint = tileCenter.Value;
                GenerateAutomaticPath(_wirePlacer.GhostWire, _startPoint.Value, endPoint, false);
            }
        }
        else
        {
            if (_wirePlacer.GhostWire != null)
                Destroy(_wirePlacer.GhostWire.gameObject);
        }
        
        if (!LevelManager.Instance.CanInput)
            _isActive = false;
    }

    bool TryRaycastTile (out Vector3? tileCenter)
    {
        tileCenter = null;
        bool success = Physics.Raycast(
            Camera.main.ScreenPointToRay(Input.mousePosition),
            out RaycastHit hit,
            100f,
            tileLayer
        );
        if (success)
            tileCenter = hit.transform.position;
        return success;
    }

    void GenerateAutomaticPath (Wire wire, Vector3 startPoint, Vector3 endPoint, bool isFinal)
    {
        Vector3Int start = _gridGenerator.WorldToGrid(startPoint);
        Vector3Int end = _gridGenerator.WorldToGrid(endPoint);

        HashSet<Vector3Int> closed = new();
        List<Node> open = new() { new Node(start) };

        Dictionary<Vector3Int, Node> allNodes = new();
        allNodes[start] = open[0];

        while (open.Count > 0)
        {
            Node current = open.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();
            open.Remove(current);
            closed.Add(current.Position);

            if (current.Position == end)
            {
                List<Vector3> path = new();
                Node temp = current;
                while (temp != null)
                {
                    Vector3 worldPos = _gridGenerator.GridToWorld(temp.Position);
                    GridTile tile = _gridGenerator.GetTileByCenterPoint(worldPos);
                    if (tile.IsObstacle || (!_localVisitedTiles.Contains(worldPos) && _wirePlacer.VisitedTiles.Contains(worldPos) && worldPos != startPoint))
                    {
                        if (isFinal)
                            ClearCurrentLine(wire);
                        return;
                    }
                    path.Insert(0, worldPos);
                    temp = temp.Parent;
                }

                UpdateLineRenderer(wire, path, isFinal);
                return;
            }

            foreach (Vector3Int offset in new Vector3Int[]
                     {
                         new(1, 0, 0), new(-1, 0, 0),
                         new(0, 0, 1), new(0, 0, -1)
                     })
            {
                Vector3Int neighborPos = current.Position + offset;

                if (closed.Contains(neighborPos))
                    continue;

                if (!_gridGenerator.TryGetTile(neighborPos, out GridTile neighborTile))
                    continue;
                
                bool isEndTile = neighborPos == end;
                if (!isEndTile && (neighborTile.IsGenerator || neighborTile.IsObstacle || neighborTile.IsStructure || neighborTile.IsPole))
                    continue;

                Vector3 worldPos = _gridGenerator.GridToWorld(neighborPos);
                if (!_localVisitedTiles.Contains(worldPos) && _wirePlacer.VisitedTiles.Contains(worldPos))
                    continue;

                float gCost = current.GCost + 1f;
                float hCost = Vector3Int.Distance(neighborPos, end);

                if (!allNodes.TryGetValue(neighborPos, out Node neighbor))
                {
                    neighbor = new Node(neighborPos);
                    allNodes[neighborPos] = neighbor;
                }

                if (gCost < neighbor.GCost || neighbor.Parent == null)
                {
                    neighbor.GCost = gCost;
                    neighbor.HCost = hCost;
                    neighbor.Parent = current;

                    if (!open.Contains(neighbor))
                        open.Add(neighbor);
                }
            }
        }
        
        if (isFinal)
            ClearCurrentLine(wire);
    }

    void UpdateLineRenderer(Wire wire, List<Vector3> newTiles, bool isFinal)
    {
        if (!isFinal && _wirePoints.Count != newTiles.Count)
            LevelManager.Instance.TriggerOnGhostWireUpdated();
        
        bool isConnectedToPole = _gridGenerator.GetTileByCenterPoint(newTiles[0]).IsPole
                                 || _gridGenerator.GetTileByCenterPoint(newTiles[^1]).IsPole;
        float multiplier = isConnectedToPole ? 0.75f : 1f;
        float cost = (_wirePoints.Count - 1 ) * multiplier;
        int roundedCost = Mathf.RoundToInt(cost);
        if (isFinal && !LevelManager.Instance.TryConsumeWire(roundedCost))
        {
            ClearCurrentLine(wire);
            return;
        }
        
        _wirePoints.Clear();
        Vector3 lastTile = newTiles[^1];
        foreach (Vector3 tile in newTiles)
        {
            _wirePoints.Add(tile);
            if (isFinal && lastTile != tile)
                _localVisitedTiles.Add(tile);
        }
        
        wire.LineRenderer.positionCount = _wirePoints.Count;

        for (int index = 0; index < _wirePoints.Count; index++)
        {
            Vector3 point = _wirePoints[index];
            wire.LineRenderer.positionCount = _wirePoints.Count;
            wire.LineRenderer.SetPosition(index,point + Vector3.up * 0.5f);
        }
        
        if (isFinal)
        {
            _wirePlacer.Wires.Add(wire);
            wire.Setup(_gridGenerator);
            
            foreach (Vector3 tile in _localVisitedTiles)
                _wirePlacer.VisitedTiles.Add(tile);
            _wirePlacer.VisitedTiles.Remove(wire.StartPoint);
            _localVisitedTiles.Clear();

            wire.Price = roundedCost;
            wire.SetWireMesh();
            
            LevelManager.Instance.RecalculatePowerFlow(true);
        }
    }

    void ClearCurrentLine (Wire wire)
    {
        Reset();
        _wirePoints.Clear();
        
        if (wire != null)
            Destroy(wire.gameObject);
    }
}

class Node
{
    public Vector3Int Position;
    public float GCost;
    public float HCost;
    public float FCost => GCost + HCost;
    public Node Parent;

    public Node (Vector3Int position)
    {
        Position = position;
    }
}

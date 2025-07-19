using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WirePlacer : MonoBehaviour
{
    [SerializeField] GridGenerator gridGenerator;
    [SerializeField] Wire wirePrefab;
    [SerializeField] Wire ghostWirePrefab;
    [SerializeField] LayerMask tileLayer;

    List<Vector3> _wirePoints = new();
    HashSet<Vector3> _visitedTiles = new();
    List<Wire> _wires = new();

    bool _isPlacing;
    Vector3? _startPoint;
    Wire _currentWire;
    Wire _ghostWire;

    void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (TryRaycastTile(out Vector3? tileCenter))
            {
                if (tileCenter == null)
                    return;

                GridTile tile = gridGenerator.GetTileByCenterPoint(tileCenter.Value);
                if (tile == null || tile.IsObstacle)
                    return;
                
                if (_startPoint == null)
                {
                    if (!tile.IsGenerator && !tile.IsPole)
                        return;
                    
                    _startPoint = tileCenter;

                    _isPlacing = true;
                    _currentWire = Instantiate(wirePrefab, transform.position, Quaternion.identity, transform);
                    return;
                }

                if (!tile.IsStructure && !tile.IsPole && !tile.IsGenerator)
                {
                    ClearCurrentLine(_currentWire);
                    return;
                }
                
                Vector3 endPoint = tileCenter.Value;
                GeneratePath(_currentWire, _startPoint.Value, endPoint, true);
                _startPoint = null;
                _currentWire = null;
                _isPlacing = false;
            }
        }
        
        if (_isPlacing)
        {
            if (_ghostWire == null)
                _ghostWire = Instantiate(ghostWirePrefab, transform.position, Quaternion.identity, transform);
        
            if (TryRaycastTile(out Vector3? tileCenter))
            {
                if (tileCenter == null)
                    return;
                Vector3 endPoint = tileCenter.Value;
                GeneratePath(_ghostWire, _startPoint.Value, endPoint, false);
            }
        }
        else
        {
            if (_ghostWire != null)
                Destroy(_ghostWire.gameObject);
        }

        if (Input.GetMouseButtonDown(1))
            ClearWires();
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

    void GeneratePath (Wire wire, Vector3 startPoint, Vector3 endPoint, bool isFinal)
    {
        Vector3Int start = gridGenerator.WorldToGrid(startPoint);
        Vector3Int end = gridGenerator.WorldToGrid(endPoint);

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
                    Vector3 worldPos = gridGenerator.GridToWorld(temp.Position);
                    if (_visitedTiles.Contains(worldPos) && worldPos != startPoint)
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

                if (!gridGenerator.TryGetTile(neighborPos, out GridTile neighborTile))
                    continue;
                
                bool isEndTile = neighborPos == end;
                if (!isEndTile && (neighborTile.IsGenerator || neighborTile.IsObstacle || neighborTile.IsStructure || neighborTile.IsPole))
                    continue;

                Vector3 worldPos = gridGenerator.GridToWorld(neighborPos);
                if ( _visitedTiles.Contains(worldPos))
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
        if (isFinal && !LevelManager.Instance.TryConsumeWire(newTiles.Count - 1))
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
                _visitedTiles.Add(tile);
        }
        
        wire.LineRenderer.positionCount = _wirePoints.Count;

        for (int index = 0; index < _wirePoints.Count; index++)
        {
            Vector3 point = _wirePoints[index];
            wire.LineRenderer.positionCount = _wirePoints.Count;
            wire.LineRenderer.SetPosition(index,point + Vector3.up * 3);
        }
        
        if (isFinal)
        {
            _wires.Add(wire);
            wire.Setup(gridGenerator);
            LevelManager.Instance.RecalculatePowerFlow();
        }
    }

    void ClearWires ()
    {
        _wirePoints.Clear();
        _visitedTiles.Clear();
        
        foreach (Wire wire in _wires)
            Destroy(wire.gameObject);
        _wires.Clear();
        
        LevelManager.Instance.ClearWires();
        LevelManager.Instance.RecalculatePowerFlow();

        if (_ghostWire != null)
        {
            ClearCurrentLine(_currentWire);
            ClearCurrentLine(_ghostWire);
        }
    }

    void ClearCurrentLine (Wire wire)
    {
        _isPlacing = false;
        _startPoint = null;
        _wirePoints.Clear();
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
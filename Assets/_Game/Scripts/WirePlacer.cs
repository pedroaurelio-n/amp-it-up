using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WirePlacer : MonoBehaviour
{
    [SerializeField] GridGenerator gridGenerator;
    [SerializeField] LineRenderer lineRendererPrefab;
    [SerializeField] LineRenderer ghostLinePrefab;
    [SerializeField] LayerMask tileLayer;

    List<Vector3> _wirePoints = new();
    HashSet<Vector3> _visitedTiles = new();
    List<LineRenderer> _lineRenderers = new();

    bool _isPlacing;
    Vector3? _startPoint = null;
    LineRenderer _currentLine = null;
    LineRenderer _ghostLine = null;

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
                    _currentLine = Instantiate(lineRendererPrefab, transform.position, Quaternion.identity, transform);
                    _currentLine.SetPosition(0, _startPoint.Value);
                    _currentLine.SetPosition(1, _startPoint.Value + new Vector3(0.1f, 0.1f, 0.1f));
                    return;
                }

                if (!tile.IsStructure && !tile.IsPole)
                {
                    ClearCurrentLine(_currentLine);
                    return;
                }
                
                Vector3 endPoint = tileCenter.Value;
                GeneratePath(_currentLine, _startPoint.Value, endPoint, true);
                _startPoint = null;
                _currentLine = null;
                _isPlacing = false;
            }
        }
        
        if (_isPlacing)
        {
            if (_ghostLine == null)
                _ghostLine = Instantiate(ghostLinePrefab, transform.position, Quaternion.identity, transform);
        
            if (TryRaycastTile(out Vector3? tileCenter))
            {
                if (tileCenter == null)
                    return;
                Vector3 endPoint = tileCenter.Value;
                GeneratePath(_ghostLine, _startPoint.Value, endPoint, false);
            }
        }
        else
        {
            if (_ghostLine != null)
                Destroy(_ghostLine.gameObject);
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

    void GeneratePath (LineRenderer line, Vector3 startPoint, Vector3 endPoint, bool isFinal)
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
                            ClearCurrentLine(line);
                        return;
                    }
                    path.Insert(0, worldPos);
                    temp = temp.Parent;
                }

                UpdateLineRenderer(line, path, isFinal);
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
            ClearCurrentLine(line);
    }

    void UpdateLineRenderer(LineRenderer line, List<Vector3> newTiles, bool isFinal)
    {
        _wirePoints.Clear();
        Vector3 lastTile = newTiles[^1];
        foreach (Vector3 tile in newTiles)
        {
            _wirePoints.Add(tile);
            if (isFinal && lastTile != tile)
                _visitedTiles.Add(tile);
        }
        
        line.positionCount = _wirePoints.Count;
        line.SetPositions(_wirePoints.ToArray());
        
        if (isFinal)
            _lineRenderers.Add(line);
    }

    void ClearWires ()
    {
        _wirePoints.Clear();
        _visitedTiles.Clear();
        
        foreach (LineRenderer line in _lineRenderers)
            Destroy(line.gameObject);
        _lineRenderers.Clear();
    }

    void ClearCurrentLine (LineRenderer line)
    {
        _isPlacing = false;
        _startPoint = null;
        _wirePoints.Clear();
        Destroy(line.gameObject);
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
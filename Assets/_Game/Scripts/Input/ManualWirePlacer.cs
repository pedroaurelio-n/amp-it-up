using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ManualWirePlacer : MonoBehaviour
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
                    
                    _wirePlacer.CurrentWire = Instantiate(wirePrefab, transform.position, Quaternion.identity, _wirePlacer.transform);
                    _wirePlacer.CurrentWire.LineRenderer.positionCount = 0;
                    _wirePlacer.GhostWire = Instantiate(ghostWirePrefab, transform.position, Quaternion.identity, _wirePlacer.transform);
                    _wirePlacer.GhostWire.LineRenderer.SetPosition(0, _startPoint.Value + Vector3.up * 0.5f);
                    TryStartOrExtendWire();
                    return;
                }
                
                TryStartOrExtendWire();
            }
        }
        
        if (IsPlacing)
        {
            if (TryRaycastTile(out Vector3? tileCenter))
            {
                if (tileCenter == null)
                    return;
                Vector3 endPoint = tileCenter.Value;
                endPoint.y = 0f;
                if (!_localVisitedTiles.Contains(endPoint) && !_wirePlacer.VisitedTiles.Contains(endPoint) && IsAdjacent(endPoint))
                {
                    _wirePlacer.GhostWire.LineRenderer.positionCount = _wirePoints.Count + 1;
                    for (int i = 0; i < _wirePoints.Count; i++)
                        _wirePlacer.GhostWire.LineRenderer.SetPosition(i, _wirePoints[i] + Vector3.up * 0.5f);
                    _wirePlacer.GhostWire.LineRenderer.SetPosition(_wirePoints.Count, endPoint + Vector3.up * 0.5f);
                }
                else
                    _wirePlacer.GhostWire.LineRenderer.positionCount = _wirePoints.Count;
            }
        }
        else
        {
            if (_wirePlacer.GhostWire != null && _wirePoints.Count > 0)
            {
                _wirePlacer.GhostWire.LineRenderer.positionCount = _wirePoints.Count;
            }
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
    
    void TryStartOrExtendWire ()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, tileLayer))
        {
            Vector3 tileCenter = hit.collider.transform.position;
            tileCenter.y = 0f;
            GridTile tile = _gridGenerator.GetTileByCenterPoint(tileCenter);

            if (!IsPlacing)
            {
                IsPlacing = true;
                _wirePoints.Clear();
                AddPoint(tileCenter, false);
            }
            else
            {
                if (!_localVisitedTiles.Contains(tileCenter) && !_wirePlacer.VisitedTiles.Contains(tileCenter) && IsAdjacent(tileCenter))
                {
                    if (!tile.IsStructure && !tile.IsPole && !tile.IsGenerator)
                        AddPoint(tileCenter, false);
                    else
                    {
                        if (tile.IsPole || tile.IsGenerator)
                        {
                            tile.TryGetComponent(out Generator generator);
                            tile.TryGetComponent(out Pole pole);
                            tile.TryGetComponent(out Structure structure);
                            generator?.Click();
                            pole?.Click();
                            structure?.Click();
                        }
                        AddPoint(tileCenter, true);
                    }
                }
                else
                {
                    LevelManager.Instance.TriggerOnWireImpossible();
                }
            }
        }
    }

    void AddPoint(Vector3 point, bool isFinal)
    {
        point.y = 0f;
        _wirePoints.Add(point);
        _localVisitedTiles.Add(point);
        UpdateLineRenderer(_wirePlacer.GhostWire, isFinal);
        if (!isFinal)
            LevelManager.Instance.TriggerOnGhostWireUpdated();
    }
    
    bool IsAdjacent(Vector3 point)
    {
        if (_wirePoints.Count == 0) return true;

        Vector3 last = _wirePoints[^1];
        Vector3 delta = point - last;
        
        return Mathf.Approximately(Mathf.Abs(delta.x), 2.5f) && Mathf.Approximately(delta.y, 0f) && Mathf.Approximately(delta.z, 0f)
               || Mathf.Approximately(Mathf.Abs(delta.z), 2.5f) && Mathf.Approximately(delta.x, 0f) && Mathf.Approximately(delta.y, 0f);
    }

    void UpdateLineRenderer (Wire wire, bool isFinal)
    {
        for (int index = 0; index < _wirePoints.Count; index++)
        {
            Vector3 point = _wirePoints[index];
            wire.LineRenderer.positionCount = _wirePoints.Count;
            wire.LineRenderer.SetPosition(index,point + Vector3.up * 0.5f);
        }

        if (!isFinal)
            return;
        
        bool isConnectedToPole = _gridGenerator.GetTileByCenterPoint(_wirePoints[0]).IsPole
                                 || _gridGenerator.GetTileByCenterPoint(_wirePoints[^1]).IsPole;
        float multiplier = isConnectedToPole ? 0.75f : 1f;
        float cost = (_wirePoints.Count - 1 ) * multiplier;
        int roundedCost = Mathf.RoundToInt(cost);
        if (!LevelManager.Instance.TryConsumeWire(roundedCost))
        {
            ClearCurrentLine(wire);
            return;
        }
        
        _wirePlacer.CurrentWire.LineRenderer.positionCount = _wirePlacer.GhostWire.LineRenderer.positionCount;
        for (int i = 0; i < _wirePlacer.GhostWire.LineRenderer.positionCount; i++)
            _wirePlacer.CurrentWire.LineRenderer.SetPosition(i, _wirePlacer.GhostWire.LineRenderer.GetPosition(i));
        
        _wirePlacer.CurrentWire.Setup(_gridGenerator);
        _wirePlacer.Wires.Add(_wirePlacer.CurrentWire);

        foreach (Vector3 tile in _localVisitedTiles)
            _wirePlacer.VisitedTiles.Add(tile);
        _wirePlacer.VisitedTiles.Remove(_wirePlacer.CurrentWire.StartPoint);
        _wirePlacer.VisitedTiles.Remove(_wirePlacer.CurrentWire.EndPoint);
        //TODO pedro: multiple end points
        // _wirePlacer.VisitedTiles.Remove(_wirePlacer.CurrentWire.EndPoint);
        _localVisitedTiles.Clear();
        
        Reset();
        LevelManager.Instance.RecalculatePowerFlow(true);
        _wirePlacer.CurrentWire.Price = roundedCost;
        _wirePlacer.CurrentWire.SetWireMesh();
            
        Destroy(_wirePlacer.GhostWire.gameObject);
    }

    void ClearCurrentLine (Wire wire)
    {
        Reset();
        _wirePoints.Clear();
        Destroy(wire.gameObject);
        _wirePlacer.RefreshVisitedTiles();
    }
}

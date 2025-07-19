using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] GridTile tilePrefab;
    [SerializeField] int width = 10;
    [SerializeField] int height = 10;
    [SerializeField] float spacing = 0.1f;
    
    GridTile[,] _grid;
    Dictionary<Vector3Int, GridTile> _tiles = new Dictionary<Vector3Int, GridTile>();
    
    void Start ()
    {
        _grid = new GridTile[width, height];
        _tiles.Clear();
        
        if (transform.childCount <= 0)
            return;
        if (_tiles.Count > 0)
            return;
        
        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridTile tile = transform.GetChild(index).GetComponent<GridTile>();
                _grid[x, y] = tile;
                Vector3Int gridCoords = new(x, 0, y);
                _tiles[gridCoords] = tile;
                index++;
            }
        }
    }
    
    public bool TryGetTile(Vector3Int gridPos, out GridTile tile)
    {
        return _tiles.TryGetValue(gridPos, out tile);
    }

    public Vector3Int WorldToGrid(Vector3 worldPos) => new(
        Mathf.RoundToInt((worldPos.x - transform.position.x) / spacing),
        0,
        Mathf.RoundToInt((worldPos.z - transform.position.z) / spacing)
    );
    
    public Vector3 GridToWorld(Vector3Int gridPos) => new(gridPos.x * 2.5f, gridPos.y * 2.5f, gridPos.z * 2.5f);
    
    public GridTile GetTileByCenterPoint (Vector3 centerPoint)
    {
        Vector3Int gridPos = WorldToGrid(centerPoint);
        _tiles.TryGetValue(gridPos, out var tile);
        return tile;
    }

    [ContextMenu("Generate Grid")]
    void GenerateGrid ()
    {
        ClearGrid();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new(
                    transform.position.x + x * spacing,
                    0,
                    transform.position.z + y * spacing
                );

                GridTile tile = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                tile.name = $"Tile{x}_{y}";
            }
        }
    }

    void ClearGrid ()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}

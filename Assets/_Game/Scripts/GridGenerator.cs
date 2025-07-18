using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] GridTile tilePrefab;
    [SerializeField] int width = 10;
    [SerializeField] int height = 10;
    [SerializeField] float spacing = 0.1f;

    [ContextMenu("Generate Grid")]
    void GenerateGrid ()
    {
        ClearGrid();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new(transform.position.x + x * spacing, 0,transform.position.z +  y * spacing);
                GridTile tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
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

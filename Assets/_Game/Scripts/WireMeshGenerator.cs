using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WireMeshGenerator : MonoBehaviour
{
    [SerializeField] float wireWidth = 0.1f;
    [SerializeField] float radius = 0.3f;
    [SerializeField] int segments = 12;
    [SerializeField] bool useTubeMesh;

    public Material MeshMaterial => _meshRenderer.material;

    Mesh _mesh;
    MeshRenderer _meshRenderer;

    void Awake ()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void GenerateMesh(Vector3[] points)
    {
        if (useTubeMesh)
            GenerateTube(points);
        else
            GenerateWire(points);
    }

    void GenerateWire (Vector3[] points)
    {
        if (points == null || points.Length < 2)
        {
            Debug.LogWarning("Need at least 2 points to generate wire mesh.");
            return;
        }

        if (_mesh != null) Destroy(_mesh);
        _mesh = new Mesh();
        _mesh.name = "WireMesh";

        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward;
            if (i == 0)
                forward = (points[i + 1] - points[i]).normalized;
            else if (i == points.Length - 1)
                forward = (points[i] - points[i - 1]).normalized;
            else
                forward = ((points[i + 1] - points[i]) + (points[i] - points[i - 1])).normalized;

            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized * (wireWidth / 2f);

            vertices.Add(points[i] - right);
            vertices.Add(points[i] + right);

            float v = i / (float)(points.Length - 1);
            uvs.Add(new Vector2(0, v));
            uvs.Add(new Vector2(1, v));

            if (i < points.Length - 1)
            {
                int start = i * 2;
                triangles.Add(start);
                triangles.Add(start + 1);
                triangles.Add(start + 2);

                triangles.Add(start + 1);
                triangles.Add(start + 3);
                triangles.Add(start + 2);
            }
        }

        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.SetUVs(0, uvs);
        _mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = _mesh;
    }

    void GenerateTube(Vector3[] points)
    {
        if (points == null || points.Length < 2) return;
        if (_mesh != null) Destroy(_mesh);

        _mesh = new Mesh();
        _mesh.name = "TubeMesh";

        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        int ringLength = points.Length;
        int vertexPerRing = segments;

        for (int i = 0; i < ringLength; i++)
        {
            Vector3 forward;
            if (i < ringLength - 1)
                forward = (points[i + 1] - points[i]).normalized;
            else
                forward = (points[i] - points[i - 1]).normalized;

            // Create a rotation for this ring
            Quaternion rotation = Quaternion.LookRotation(forward);
            Vector3 center = points[i];

            for (int j = 0; j < vertexPerRing; j++)
            {
                float angle = 2 * Mathf.PI * j / vertexPerRing;
                Vector3 localPos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                Vector3 worldPos = center + rotation * localPos;
                vertices.Add(worldPos);

                // UVs for radial animation (can be improved)
                uvs.Add(new Vector2(j / (float)vertexPerRing, i / (float)(ringLength - 1)));
            }
        }

        // Connect rings with triangles
        for (int i = 0; i < ringLength - 1; i++)
        {
            for (int j = 0; j < vertexPerRing; j++)
            {
                int curr = i * vertexPerRing + j;
                int next = curr + vertexPerRing;

                int nextJ = (j + 1) % vertexPerRing;

                triangles.Add(curr);
                triangles.Add(next);
                triangles.Add(i * vertexPerRing + nextJ);

                triangles.Add(i * vertexPerRing + nextJ);
                triangles.Add(next);
                triangles.Add(next + nextJ - j); // same as next + (nextJ - j)
            }
        }

        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.SetUVs(0, uvs);
        _mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = _mesh;
    }
}

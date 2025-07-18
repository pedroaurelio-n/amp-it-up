using System;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    [SerializeField] bool isGenerator;
    [SerializeField] bool isStructure;
    [SerializeField] bool isObstacle;

    [SerializeField] Material[] materialList;
    [SerializeField] MeshRenderer mesh;

    void OnValidate ()
    {
        if (isGenerator)
            mesh.material = materialList[0];
        else if (isStructure)
            mesh.material = materialList[1];
        else if (isObstacle)
            mesh.material = materialList[2];
        else
            mesh.material = materialList[3];
    }
}
using System;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    [field: SerializeField] public bool IsGenerator { get; private set;  }
    [field: SerializeField] public bool IsStructure { get; private set;  }
    [field: SerializeField] public bool IsPole { get; private set;  }
    [field: SerializeField] public bool IsObstacle { get; private set;  }

    [SerializeField] Material[] materialList;
    [SerializeField] MeshRenderer mesh;

    void OnValidate ()
    {
        if (IsGenerator)
            mesh.material = materialList[0];
        else if (IsStructure)
            mesh.material = materialList[1];
        else if (IsPole)
            mesh.material = materialList[2];
        else if (IsObstacle)
            mesh.material = materialList[3];
        else
            mesh.material = materialList[4];
    }
}
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

    void Awake()
    {
        if (!IsGenerator && !IsStructure && !IsPole)
        {
            RemoveComponentIfExists<Generator>();
            RemoveComponentIfExists<Structure>();
            RemoveComponentIfExists<Pole>();
            return;
        }
        
        if (IsGenerator)
        {
            if (!TryGetComponent(out Generator _))
                gameObject.AddComponent<Generator>();

            RemoveComponentIfExists<Structure>();
            RemoveComponentIfExists<Pole>();
        }
        else if (IsStructure)
        {
            if (!TryGetComponent(out Structure _))
                gameObject.AddComponent<Structure>();

            RemoveComponentIfExists<Generator>();
            RemoveComponentIfExists<Pole>();
        }
        else if (IsPole)
        {
            if (!TryGetComponent(out Pole _))
                gameObject.AddComponent<Pole>();

            RemoveComponentIfExists<Generator>();
            RemoveComponentIfExists<Structure>();
        }
    }

    void RemoveComponentIfExists<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component != null)
        {
            Destroy(component);
        }
    }

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
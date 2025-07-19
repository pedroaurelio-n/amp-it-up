using UnityEngine;

public class Generator : MonoBehaviour
{
    void Start ()
    {
        LevelManager.Instance.RegisterGenerator(this);
    }
}
using System;
using UnityEngine;

public class HUD : MonoBehaviour
{
    Canvas _canvas;

    void Awake ()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = Camera.main;
    }
}

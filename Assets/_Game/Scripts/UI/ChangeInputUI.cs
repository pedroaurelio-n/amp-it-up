using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeInputUI : MonoBehaviour
{
    WirePlacer _wirePlacer;
    
    public void ChangeInput ()
    {
        if (_wirePlacer == null)
            _wirePlacer = FindObjectOfType<WirePlacer>();
        
        _wirePlacer.ChangeInput();
    }
}

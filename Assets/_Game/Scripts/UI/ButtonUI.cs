using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonUI : MonoBehaviour
{
    [SerializeField] GameObject objectToActivate;
    [SerializeField] GameObject objectToDeactivate;

    public void Action ()
    {
        objectToActivate.SetActive(true);
        objectToDeactivate.SetActive(false);
    }
}

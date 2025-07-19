using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WirePlacer : MonoBehaviour
{
    [SerializeField] GridGenerator gridGenerator;
    [SerializeField] InputModeUI inputModeUI;
    [SerializeField] AutoWirePlacer autoWirePlacer;
    [SerializeField] ManualWirePlacer manualWirePlacer;
    [SerializeField] WireUI wireUI;
    
    public Wire CurrentWire { get; set; }
    public Wire GhostWire { get; set; }
    
    public HashSet<Vector3> VisitedTiles { get; private set; } = new();
    public List<Wire> Wires { get; private set; } = new();

    InputMode CurrentInput
    {
        get => _currentInput;
        set
        {
            _currentInput = value;
            inputModeUI.SetInputModeText(_currentInput.ToString());
            if (CurrentInput == InputMode.Auto)
            {
                autoWirePlacer.gameObject.SetActive(true);
                manualWirePlacer.gameObject.SetActive(false);
            }
            else if (CurrentInput == InputMode.Manual)
            {
                autoWirePlacer.gameObject.SetActive(false);
                manualWirePlacer.gameObject.SetActive(true);
            }
        }
    }

    InputMode _currentInput;

    void Start ()
    {
        CurrentInput = InputMode.Auto;
        autoWirePlacer.Setup(this, gridGenerator);
        manualWirePlacer.Setup(this, gridGenerator);
    }

    void Update ()
    {
        wireUI.SetMinusWireText(false);
        if (GhostWire != null && GhostWire.LineRenderer.positionCount > 1)
        {
            bool isConnectedToPole = gridGenerator.GetTileByCenterPoint(GhostWire.LineRenderer.GetPosition(0)).IsPole
                                     || gridGenerator.GetTileByCenterPoint(GhostWire.LineRenderer.GetPosition(
                                                 GhostWire.LineRenderer.positionCount - 1)
                                         ).IsPole;
            float multiplier = isConnectedToPole ? 0.75f : 1f;
            float cost = (GhostWire.LineRenderer.positionCount - 1 ) * multiplier;
            int roundedCost = Mathf.RoundToInt(cost);
            wireUI.SetMinusWireText(true, isConnectedToPole, $"-{roundedCost}");
        }
        
        if (!LevelManager.Instance.CanInput)
            return;
        
        if (Input.GetMouseButtonDown(2) || Input.mouseScrollDelta.y != 0)
        {
            if (!autoWirePlacer.IsPlacing && !manualWirePlacer.IsPlacing)
            {
                if (CurrentInput == InputMode.Auto)
                    CurrentInput = InputMode.Manual;
                else if (CurrentInput == InputMode.Manual)
                    CurrentInput = InputMode.Auto;
            }
        }

        if (Input.GetMouseButtonDown(1))
            ClearPreviousWire();
    }
    
    public void RefreshVisitedTiles ()
    {
        VisitedTiles.Clear();
        foreach (Wire wire in Wires)
        {
            for (int i = 1; i < wire.LineRenderer.positionCount - 1; i++)
            {
                Vector3 position = wire.LineRenderer.GetPosition(i);
                VisitedTiles.Add(new Vector3(position.x, 0, position.z));
            }
        }
    }

    void ClearPreviousWire ()
    {
        if (Wires.Count <= 0)
        {
            if (GhostWire != null)
            {
                ClearCurrentLine(CurrentWire);
                ClearCurrentLine(GhostWire);
            }
            return;
        }

        autoWirePlacer.ClearWirePoints();
        manualWirePlacer.ClearWirePoints();
        
        if (GhostWire != null)
        {
            ClearCurrentLine(CurrentWire);
            ClearCurrentLine(GhostWire);
            return;
        }

        Wire previousWire = Wires[^1];
        Wires.Remove(previousWire);

        RefreshVisitedTiles();

        LevelManager.Instance.DeletePreviousWire(previousWire);
        LevelManager.Instance.RecalculatePowerFlow();
        
        Destroy(previousWire.gameObject);
    }

    void ClearCurrentLine (Wire wire)
    {
        autoWirePlacer.Reset();
        manualWirePlacer.Reset();
        if (wire != null)
            Destroy(wire.gameObject);
    }
}

public enum InputMode
{
    Auto,
    Manual
}
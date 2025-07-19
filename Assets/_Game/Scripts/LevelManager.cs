using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    
    public event Action OnLevelCompleted;

    [SerializeField] int levelNumber;

    [Header("Wire Config")]
    [SerializeField] int startingWireLength = 100;
    
    [Header("References")]
    [SerializeField] WireUI wireUI;
    [SerializeField] private List<Generator> generators = new();
    [SerializeField] private List<Structure> structures = new();
    [SerializeField] private List<Wire> allWires = new();
    
    public int RemainingWireLength => startingWireLength - _usedWireLength;
    public bool CanInput { get; private set; } = true;
    
    int _usedWireLength;
    
    void Awake ()
    {
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;
    }

    void Start ()
    {
        UpdateWireUI();
    }
    
    public bool TryConsumeWire (int length)
    {
        // if (_usedWireLength + length > startingWireLength)
        //     return false;
        
        _usedWireLength += length;
        UpdateWireUI();
        return true;
    }

    public void DeletePreviousWire (Wire wire)
    {
        allWires.Remove(wire);
        _usedWireLength -= wire.Price;
        UpdateWireUI();
    }

    void UpdateWireUI ()
    {
        wireUI?.SetCurrentWireRemainingText(RemainingWireLength.ToString(), startingWireLength.ToString());
    }
    
    public void RegisterGenerator (Generator gen) => generators.Add(gen);
    public void RegisterStructure (Structure s) => structures.Add(s);
    public void RegisterWire (Wire w) => allWires.Add(w);
    
    public void RecalculatePowerFlow ()
    {
        HashSet<Structure> poweredStructures = new();
        HashSet<Wire> poweredWires = new(); 

        foreach (Generator generator in generators)
        {
            List<Wire> wires = PowerPathFinder.SetPoweredWiresFromGenerator(generator, allWires);
            foreach (Wire wire in wires)
                poweredWires.Add(wire);
            List<Structure> reachable = PowerPathFinder.FindStructuresConnected(generator, allWires);
            foreach (Structure s in reachable)
                poweredStructures.Add(s);
        }

        foreach (Structure s in structures)
            s.SetPowered(poweredStructures.Contains(s));
        
        foreach (Wire wire in allWires)
            wire.SetPoweredState(poweredWires.Contains(wire));

        if (poweredStructures.Count != structures.Count || RemainingWireLength < 0)
            return;

        CanInput = false;
        OnLevelCompleted?.Invoke();
    }
}

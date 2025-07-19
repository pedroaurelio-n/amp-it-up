using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Wire Config")]
    [SerializeField] int startingWireLength = 100;
    
    [Header("References")]
    [SerializeField] WireUI wireUI;
    [SerializeField] private List<Generator> generators = new();
    [SerializeField] private List<Structure> structures = new();
    [SerializeField] private List<Wire> allWires = new();
    
    public int RemainingWireLength => startingWireLength - _usedWireLength;
    
    int _usedWireLength;
    
    void Awake ()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    void Start ()
    {
        UpdateWireUI();
    }
    
    public bool TryConsumeWire (int length)
    {
        if (_usedWireLength + length > startingWireLength)
            return false;
        
        _usedWireLength += length;
        UpdateWireUI();
        return true;
    }

    public void ClearWires ()
    {
        allWires.Clear();
        _usedWireLength = 0;
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

        foreach (Generator generator in generators)
        {
            PowerPathFinder.SetPoweredWiresFromGenerator(generator, allWires);
            List<Structure> reachable = PowerPathFinder.FindStructuresConnected(generator, allWires);
            foreach (Structure s in reachable)
                poweredStructures.Add(s);
        }

        foreach (Structure s in structures)
            s.SetPowered(poweredStructures.Contains(s));
    }
}

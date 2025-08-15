using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    
    public event Action OnLevelCompleted;
    public event Action OnWirePowered;
    public event Action OnStructurePowered;
    public event Action OnWireDestroyed;
    public event Action OnWirePlaced;
    public event Action OnGhostWireUpdated;
    public event Action OnWireImpossible;
    public event Action OnVictory;
    public event Action OnDefeat;
    public event Action OnClickEntity;

    [SerializeField] int levelNumber;

    [Header("Wire Config")]
    [SerializeField] int startingWireLength = 100;
    
    [Header("References")]
    [SerializeField] GameObject hoverCamera;
    [SerializeField] GameObject tutorial;
    [SerializeField] MeshRenderer[] gridView;
    [SerializeField] WireUI wireUI;
    [SerializeField] private List<Generator> generators = new();
    [SerializeField] private List<Structure> structures = new();
    [SerializeField] private List<Pole> poles = new();
    [SerializeField] private List<Wire> allWires = new();
    
    public int RemainingWireLength => startingWireLength - _usedWireLength;
    public bool CanInput { get; private set; } = true;
    
    GoalUI _goalUI;
    int _usedWireLength;
    
    void Awake ()
    {
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;
        
        _goalUI = FindObjectOfType<GoalUI>();
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
    public void RegisterStructure (Structure s)
    {
        structures.Add(s);
        _goalUI.SetStructuresText($"{0}", structures.Count.ToString());
    }

    public void RegisterPole (Pole p) => poles.Add(p);
    public void RegisterWire (Wire w) => allWires.Add(w);
    
    public void RecalculatePowerFlow (bool isClick)
    {
        HashSet<Structure> poweredStructures = new();
        HashSet<Pole> poweredPoles = new();
        HashSet<Wire> poweredWires = new(); 

        foreach (Generator generator in generators)
        {
            List<Wire> wires = PowerPathFinder.SetPoweredWiresFromGenerator(generator, allWires);
            foreach (Wire wire in wires)
                poweredWires.Add(wire);
            List<Structure> reachable = PowerPathFinder.FindStructuresConnected(generator, allWires);
            foreach (Structure s in reachable)
                poweredStructures.Add(s);
            List<Pole> reachablePole = PowerPathFinder.FindPolesConnected(generator, allWires);
            foreach (Pole p in reachablePole)
                poweredPoles.Add(p);
        }

        foreach (Structure s in structures)
            s.SetPowered(poweredStructures.Contains(s));
        
        foreach (Pole p in poles)
            p.SetPowered(poweredPoles.Contains(p));
        
        foreach (Wire wire in allWires)
            wire.SetPoweredState(poweredWires.Contains(wire));
        
        if (isClick)
        {
            if (poweredWires.Count > 0)
                OnWirePowered?.Invoke();
            else
                OnWirePlaced?.Invoke();
        }
        
        _goalUI.SetStructuresText(poweredStructures.Count.ToString(), structures.Count.ToString());

        if (poweredStructures.Count != structures.Count)
            return;

        if (RemainingWireLength < 0)
        {
            Invoke(nameof(Defeat), 1f);
            return;
        }

        CanInput = false;
        
        Invoke(nameof(Win), 1f);
    }

    void Defeat ()
    {
        OnDefeat?.Invoke();
        CameraShake.Instance.TriggerShake(0.4f, 0.22f);
    }

    void Win ()
    {
        hoverCamera.SetActive(true);
        tutorial.SetActive(false);
        foreach (MeshRenderer mesh in gridView)
        {
            mesh.enabled = false;
        }
        PlayerPrefs.SetInt($"Level_{levelNumber + 1}", 1);
        PlayerPrefs.Save();
        OnVictory?.Invoke();
        OnLevelCompleted?.Invoke();
    }
    
    public void TriggerOnClickEntity ()
    {
        OnClickEntity?.Invoke();
    }

    public void TriggerStructurePowered ()
    {
        OnStructurePowered?.Invoke();
    }
    
    public void TriggerOnGhostWireUpdated ()
    {
        OnGhostWireUpdated?.Invoke();
    }
    
    public void TriggerOnWireImpossible ()
    {
        OnWireImpossible?.Invoke();
        CameraShake.Instance.TriggerShake(0.25f, 0.1f);
    }

    public void TriggerWireDestroyed ()
    {
        OnWireDestroyed?.Invoke();
    }
}

using System.Collections.Generic;
using Unity.VisualScripting;

public static class PowerPathFinder
{
    public static List<Structure> FindStructuresConnected(Generator generator, List<Wire> wires)
    {
        HashSet<GridTile> visited = new();
        Queue<GridTile> queue = new();
        List<Structure> connectedStructures = new();

        GridTile root = generator.GetComponent<GridTile>();
        queue.Enqueue(root);
        visited.Add(root);

        while (queue.Count > 0)
        {
            GridTile current = queue.Dequeue();

            foreach (Wire wire in wires)
            {
                GridTile neighbor;
                
                if (wire.GetStartTile() == current)
                    neighbor = wire.GetEndTile();
                else if (wire.GetEndTile() == current)
                    neighbor = wire.GetStartTile();
                else
                    continue;

                if (neighbor == null || visited.Contains(neighbor))
                    continue;
                
                visited.Add(neighbor);

                if (neighbor.IsStructure)
                    connectedStructures.Add(neighbor.GetComponent<Structure>());
                else if (neighbor.IsPole || neighbor.IsGenerator)
                    queue.Enqueue(neighbor);
            }
        }

        return connectedStructures;
    }
    
    public static void SetPoweredWiresFromGenerator(Generator generator, List<Wire> allWires)
    {
        HashSet<GridTile> visitedTiles = new();
        Queue<GridTile> queue = new();
        HashSet<Wire> evaluatedWires = new();
        
        GridTile root = generator.GetComponent<GridTile>();
        queue.Enqueue(root);
        visitedTiles.Add(root);
        
        while (queue.Count > 0)
        {
            GridTile current = queue.Dequeue();
            
            foreach (Wire wire in allWires)
            {
                if (!evaluatedWires.Contains(wire))
                    wire.SetPoweredState(false);
                
                GridTile startTile = wire.GetStartTile();
                GridTile endTile = wire.GetEndTile();
                
                bool connectsCurrent = startTile == current || endTile == current;
                GridTile neighbor = startTile == current
                    ? endTile
                    : endTile == current
                        ? startTile
                        : null;
                
                if (!connectsCurrent || neighbor == null || visitedTiles.Contains(neighbor))
                    continue;

                wire.SetPoweredState(true);
                evaluatedWires.Add(wire);
                visitedTiles.Add(neighbor);
                if (neighbor.IsPole || neighbor.IsGenerator)
                    queue.Enqueue(neighbor);
            }
        }
    }
}
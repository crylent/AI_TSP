using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AI_labs.Network;

public class Route: List<int>
{
    public int Length { get; private set; }

    public Route(int capacity, int home): base(capacity)
    {
        Add(home);
    }
            
    private Route(Route route): base(route)
    {
        Length = route.Length;
    }

    public Route Copy => new(this);

    public void Reset()
    {
        var home = this[0];
        Clear();
        Add(home);
        Length = 0;
    }

    public void Add(int node, int length)
    {
        Add(node);
        Length += length;
    }

    public void Swap(int nodeA, int nodeB, Network<int> network)
    {
        (this[nodeA], this[nodeB]) = (this[nodeB], this[nodeA]);
        RecalculateLength(network);
    }

    public void RecalculateLength(Network<int> network)
    {
        Length = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var pathLength = network.GetValue(this[i], this[i + 1]);
            if (pathLength == Network<int>.NoPath) pathLength = 999999;
            Length += pathLength;
        }
    }

    public static Route Random(int capacity, int home)
    {
        var route = new Route(capacity, home);
        var nodesToAdd = Enumerable.Range(0, capacity - 1).ToList();
        nodesToAdd.Remove(home);
        while (nodesToAdd.Count > 0)
        {
            var next = nodesToAdd[System.Random.Shared.Next(0, nodesToAdd.Count)];
            route.Add(next);
            nodesToAdd.Remove(next);
        }
        route.Add(home); // close the route
        return route;
    }
}
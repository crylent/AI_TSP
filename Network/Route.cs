using System.Collections.Generic;

namespace AI_labs.Network;

public class Route: List<int>
{
    public int Length;

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
}
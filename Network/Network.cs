using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AI_labs.Network;

public class Network<T> where T: INumber<T>
{
    protected readonly List<List<T>> Matrix = new();
    public int Size => Matrix.Count;

    public static readonly T? NoPath = default;

    protected Network(int size = 0, T? defaultValue = default)
    {
        for (var i = 0; i < size; i++)
        {
            Matrix.Add(Enumerable.Repeat(defaultValue, i).ToList()!);
        }
    }

    public T GetValue(int nodeA, int nodeB)
    {
        var (node1, node2) = OrderIndices(nodeA, nodeB);
        var length = Matrix[node1][node2];
        return length;
    }
    
    protected void ForEachPath(Func<int, int, object> func)
    {
        for (var i = 0; i < Size; i++)
        {
            for (var j = 0; j < Matrix[i].Count; j++)
            {
                if (GetValue(i, j) != NoPath) func(i, j);
            }
        }
    }

    protected static (int, int) OrderIndices(int nodeA, int nodeB)
    {
        return nodeA > nodeB ? (nodeA, nodeB) : (nodeB, nodeA);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;

namespace AI_labs.Network;

[DataContract]
public class Network<T> where T: INumber<T>
{
    [DataMember] protected readonly List<List<T>> Matrix = new();
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

    public void ForEachPath(Func<int, int, T, object?> func)
    {
        for (var i = 0; i < Size; i++)
        {
            for (var j = 0; j < Matrix[i].Count; j++)
            {
                var value = GetValue(i, j);
                if (value != NoPath) func(i, j, value);
            }
        }
    }

    protected static (int, int) OrderIndices(int nodeA, int nodeB)
    {
        return nodeA > nodeB ? (nodeA, nodeB) : (nodeB, nodeA);
    }
}
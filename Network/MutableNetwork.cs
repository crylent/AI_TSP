using System.Linq;
using System.Numerics;

namespace AI_labs.Network;

public class MutableNetwork<T>: Network<T> where T : INumber<T>
{
    public MutableNetwork(int size = 0, T? defaultValue = default) : base(size, defaultValue)
    {
    }
    

    public void AddNode()
    {
        Matrix.Add(Enumerable.Repeat(NoPath, Matrix.Count).ToList()!);
    }

    public void RemoveNode(int node)
    {
        Matrix.RemoveAt(node);
        for (var i = node; i < Matrix.Count; i++)
        {
            Matrix[i].RemoveAt(i - 1);
        }
    }

    public void AddPath(int nodeA, int nodeB, T? value)
    {
        SetPath(nodeA, nodeB, value);
    }

    public void RemovePath(int nodeA, int nodeB)
    {
        SetPath(nodeA, nodeB, NoPath);
    }

    public void SetPath(int nodeA, int nodeB, T? value)
    {
        var (node1, node2) = OrderIndices(nodeA, nodeB);
        Matrix[node1][node2] = value!;
    }
}
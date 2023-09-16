using System.Collections.Generic;
using System.Linq;

namespace AI_labs;

public class Network
{
    private readonly List<List<int>> _matrix = new();

    private const int NoPath = -1;

    public Network(int size)
    {
        for (var i = 0; i < size; i++)
        {
            _matrix[i] = Enumerable.Repeat(NoPath, i).ToList();
        }
    }

    public void AddNode()
    {
        _matrix.Add(Enumerable.Repeat(NoPath, _matrix.Count).ToList());
    }

    public void RemoveNode(int node)
    {
        _matrix.RemoveAt(node);
        for (var i = node; i < _matrix.Count; i++)
        {
            _matrix[i].RemoveAt(i - 1);
        }
    }

    public void AddPath(int nodeA, int nodeB, int length)
    {
        SetPath(nodeA, nodeB, length);
    }

    public void RemovePath(int nodeA, int nodeB)
    {
        SetPath(nodeA, nodeB, NoPath);
    }

    public void SetPath(int nodeA, int nodeB, int length)
    {
        var (node1, node2) = OrderIndices(nodeA, nodeB);
        _matrix[node1][node2] = length;
    }

    public int GetLength(int nodeA, int nodeB)
    {
        var (node1, node2) = OrderIndices(nodeA, nodeB);
        return _matrix[node1][node2];
    }

    private static (int, int) OrderIndices(int nodeA, int nodeB)
    {
        return nodeA > nodeB ? (nodeA, nodeB) : (nodeB, nodeA);
    }
}
using System;
using System.Windows.Shapes;

namespace AI_labs.UI;

internal readonly struct NodePair
{
    public readonly int A;
    public readonly int B;

    // ReSharper disable once MemberCanBePrivate.Local
    public NodePair(int nodeA, int nodeB)
    {
        A = nodeA;
        B = nodeB;
    }

    public NodePair(Ellipse nodeA, Ellipse nodeB, Func<Ellipse, int> getNodeIndex) 
        : this(getNodeIndex(nodeA), getNodeIndex(nodeB)) 
    { }

    // ReSharper disable once MemberCanBePrivate.Global
    public bool Equals(NodePair other)
    {
        return A == other.A && B == other.B || A == other.B && B == other.A;
    }

    public override bool Equals(object? obj)
    {
        return obj is NodePair other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            // ReSharper disable once ArrangeRedundantParentheses
            return (A * 397) ^ B + (B * 397) ^ A;
        }
    }

    public static bool operator ==(NodePair left, NodePair right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NodePair left, NodePair right)
    {
        return !left.Equals(right);
    }
}
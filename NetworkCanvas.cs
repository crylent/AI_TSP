using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AI_labs;

public class NetworkCanvas: Canvas
{
    private const int NodeSize = 40;
    private const int NodeHalfSize = NodeSize / 2;
    private readonly SolidColorBrush _nodeDefaultColor = Brushes.Azure;
    private readonly SolidColorBrush _nodeHighlightColor = Brushes.Khaki;
    private readonly SolidColorBrush _lineDefaultColor = Brushes.Black;
    private readonly SolidColorBrush _lineHighlightColor = Brushes.DimGray;
    
    private readonly Network _network = new(0);
    private readonly List<Ellipse> _nodes = new();

    private struct NodePair
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
    }

    private readonly Dictionary<Line, NodePair> _lines = new();

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_lineFromSelectedNode == null) return;

        var position = e.GetPosition(this);
        _lineFromSelectedNode.X2 = position.X;
        _lineFromSelectedNode.Y2 = position.Y;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (_preventCanvasClick)
        {
            _preventCanvasClick = false;
            return;
        }
        
        if (_selectedNode != null) {
            _selectedNode = null;
            Children.Remove(_lineFromSelectedNode);
            _lineFromSelectedNode = null;
            return;
        }
        
        var position = e.GetPosition(this);
        
        AddNode(position);
    }

    private bool _preventCanvasClick;

    private void AddNode(Point position)
    {
        
        _network.AddNode();
        var node = new Ellipse
        {
            Width = NodeSize,
            Height = NodeSize,
            Stroke = Brushes.Black,
            Fill = _nodeDefaultColor
        };
        Children.Add(node);
        SetLeft(node, position.X - NodeHalfSize);
        SetTop(node, position.Y - NodeHalfSize);
        SetupListeners(node);
        _nodes.Add(node);
    }

    private void SetupListeners(IInputElement element)
    {
        switch (element)
        {
            case Ellipse:
                element.MouseLeftButtonDown += OnNodeClicked;
                element.MouseEnter += HighlightNode;
                element.MouseLeave += ResetNode;
                break;
            case Line:
                element.MouseLeftButtonDown += PromptLengthDialog;
                element.MouseEnter += HighlightLine;
                element.MouseLeave += ResetLine;
                break;
        }
    }

    private Ellipse? _selectedNode;
    private Line? _lineFromSelectedNode;

    private void HighlightNode(object sender, MouseEventArgs? e = null)
    {
        (sender as Ellipse)!.Fill = _nodeHighlightColor;
    }

    private void ResetNode(object sender, MouseEventArgs? e = null)
    {
        (sender as Ellipse)!.Fill = _nodeDefaultColor;
    }

    private void OnNodeClicked(object sender, MouseButtonEventArgs e)
    {
        _preventCanvasClick = true;
        var ellipse = (sender as Ellipse)!;
        switch (e.ClickCount)
        {
            case 1:
                SelectNode(ellipse);
                break;
            case 2:
                RemoveNode(ellipse);
                break;
        }
    }

    private void SelectNode(Ellipse node)
    {
        if (_selectedNode == null)
        {
            var (x, y) = (GetLeft(node) + NodeHalfSize, GetTop(node) + NodeHalfSize);
            _lineFromSelectedNode = new Line
            {
                X1 = x, Y1 = y,
                X2 = x, Y2 = y,
                Stroke = _lineDefaultColor,
                StrokeThickness = 5
            };
            Children.Add(_lineFromSelectedNode);
            SetZIndex(_lineFromSelectedNode, -1);
            _selectedNode = node;
        }
        else
        {
            AddPath(_selectedNode, node);
            _lineFromSelectedNode!.X2 = GetLeft(node) + NodeHalfSize;
            _lineFromSelectedNode.Y2 = GetTop(node) + NodeHalfSize;
            SetupListeners(_lineFromSelectedNode);
            _lines.Add(_lineFromSelectedNode, new NodePair(_selectedNode, node, GetNodeIndex));
            _lineFromSelectedNode = null;
            _selectedNode.Fill = _nodeDefaultColor;
            _selectedNode = null;
        }
    }

    private void RemoveNode(Ellipse node)
    {
        var nodeIndex = GetNodeIndex(node);
        _nodes.RemoveAt(nodeIndex);
        _network.RemoveNode(nodeIndex);
        Children.Remove(node);
        foreach (var (line, nodes) in _lines)
        {
            if (nodes.A == nodeIndex || nodes.B == nodeIndex)
            {
                Children.Remove(line);
            }
        }

        if (node != _selectedNode) return;
        Children.Remove(_lineFromSelectedNode);
        _selectedNode = null;
    }

    private void AddPath(Ellipse from, Ellipse to)
    {
        _network.AddPath(GetNodeIndex(from), GetNodeIndex(to), 1);
    }

    private void HighlightLine(object sender, MouseEventArgs e)
    {
        (sender as Line)!.Stroke = _lineHighlightColor;
    }

    private void ResetLine(object sender, MouseEventArgs e)
    {
        (sender as Line)!.Stroke = _lineDefaultColor;
    }

    private void PromptLengthDialog(object sender, MouseButtonEventArgs e)
    {
        var nodes = _lines[(sender as Line)!];
        var initialLength = _network.GetLength(nodes.A, nodes.B);
        var newLength = LengthDialog.Prompt(initialLength);
        _network.SetPath(nodes.A, nodes.B, newLength ?? initialLength);
    }

    private int GetNodeIndex(Ellipse node)
    {
        return _nodes.IndexOf(node);
    }
}
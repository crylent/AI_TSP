using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
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
    private const int LabelFontSize = 24;
    private readonly SolidColorBrush _labelColor = Brushes.LightSeaGreen;
    private readonly SolidColorBrush _translucentBlack = new(new Color { A = 0x88, R = 0x00, G = 0x00, B = 0x00 });

    private readonly DropShadowEffect _shadowEffect = new()
    {
        Color = Colors.Black,
        BlurRadius = 3,
        ShadowDepth = 3
    };

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
    private readonly Dictionary<Line, Label> _labels = new();

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
            var length = LengthDialog.Prompt();
            if (length != null) // add path with defined length
            {
                AddPath(_selectedNode, node, (int) length);
                _lineFromSelectedNode!.X2 = GetLeft(node) + NodeHalfSize;
                _lineFromSelectedNode.Y2 = GetTop(node) + NodeHalfSize;
                SetupListeners(_lineFromSelectedNode);
                _lines.Add(_lineFromSelectedNode, new NodePair(_selectedNode, node, GetNodeIndex));

                var text = new Label
                {
                    Content = length.ToString(),
                    FontSize = LabelFontSize,
                    Foreground = _labelColor,
                    FontWeight = FontWeights.Bold,
                    Background = _translucentBlack,
                    Effect = _shadowEffect
                };
                Children.Add(text);
                SetLeft(text, (_lineFromSelectedNode.X1 + _lineFromSelectedNode.X2) / 2);
                SetTop(text, (_lineFromSelectedNode.Y1 + _lineFromSelectedNode.Y2) / 2);
                _labels.Add(_lineFromSelectedNode, text);
            }
            else // cancel adding path
            {
                Children.Remove(_lineFromSelectedNode);
            }
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
        var linesToRemove = new List<Line>();
        foreach (var (line, nodes) in _lines)
        {
            if (nodes.A != nodeIndex && nodes.B != nodeIndex) continue;
            // if node A or node B is the node to remove
            Children.Remove(line);
            Children.Remove(_labels[line]);
            linesToRemove.Add(line);
        }
        
        foreach (var line in linesToRemove)
        {
            _lines.Remove(line);
            _labels.Remove(line);
        }

        if (node != _selectedNode) return;
        Children.Remove(_lineFromSelectedNode);
        _selectedNode = null;
    }

    private void AddPath(Ellipse from, Ellipse to, int length)
    {
        _network.AddPath(GetNodeIndex(from), GetNodeIndex(to), length);
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
        var line = (sender as Line)!;
        var nodes = _lines[line];
        var initialLength = _network.GetLength(nodes.A, nodes.B);
        var newLength = LengthDialog.Prompt(initialLength);
        _network.SetPath(nodes.A, nodes.B, newLength ?? initialLength);
        _labels[line].Content = newLength.ToString();
    }

    private int GetNodeIndex(Ellipse node)
    {
        return _nodes.IndexOf(node);
    }
}
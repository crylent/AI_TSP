using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using AI_labs.Files;
using AI_labs.Network;

namespace AI_labs.UI;

public class NetworkCanvas: Canvas
{
    private const int NodeSize = 40;
    private const int NodeHalfSize = NodeSize / 2;
    private readonly SolidColorBrush _nodeDefaultColor = Brushes.Azure;
    private readonly SolidColorBrush _nodeHighlightColor = Brushes.Khaki;
    private readonly SolidColorBrush _lineDefaultColor = Brushes.Black;
    private readonly SolidColorBrush _lineHighlightColor = Brushes.DimGray;
    private readonly SolidColorBrush _routeColor = Brushes.MediumVioletRed;
    private const int LabelFontSize = 24;
    private readonly SolidColorBrush _labelColor = Brushes.LightSeaGreen;
    private readonly SolidColorBrush _translucentBlack = new(new Color { A = 0x88, R = 0x00, G = 0x00, B = 0x00 });

    private readonly DropShadowEffect _shadowEffect = new()
    {
        Color = Colors.Black,
        BlurRadius = 3,
        ShadowDepth = 3
    };

    public int Test = 1;

    private readonly MutableNetwork<int> _network = new();
    public Network<int> Network => _network; // read-only
    private readonly List<Ellipse> _nodes = new();

    public int StartPoint
    {
        get;
        private set;
    }

    private readonly Dictionary<Line, NodePair> _lines = new();
    private readonly Dictionary<Line, Label> _labels = new();

    public NetworkCanvas()
    {
        Background = Brushes.Bisque;
    }

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

    public void AddNode(Point position)
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
                element.MouseRightButtonDown += PromptNodeContextMenu;
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

    private void PromptNodeContextMenu(object sender, MouseButtonEventArgs e)
    {
        var ellipse = (sender as Ellipse)!;
        var contextMenu = new ContextMenu();
        var nodeIndex = GetNodeIndex(ellipse);
        AddContextMenuItem(contextMenu, new MenuItem
        {
            Header = "Start Point",
            IsCheckable = true,
            IsChecked = StartPoint == nodeIndex
        }, (_, _) => StartPoint = nodeIndex);
        AddContextMenuItem(contextMenu, new MenuItem
        {
            Header = "Remove Node"
        }, (_, _) => RemoveNode(ellipse));
        ellipse.ContextMenu = contextMenu;
    }

    private static void AddContextMenuItem(ItemsControl menu, MenuItem item, RoutedEventHandler handler)
    {
        item.Click += handler;
        menu.Items.Add(item);
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
        
        if (nodeIndex == StartPoint) StartPoint = 0; // reset start point
        
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

    public void AddPath(int from, int to, int length)
    {
        _selectedNode = null;
        SelectNode(_nodes[from]);
        AddPath(_nodes[from], _nodes[to], length);
        _lineFromSelectedNode = null;
    }

    private void AddPath(Ellipse from, Ellipse to, int length)
    {
        _network.AddPath(GetNodeIndex(from), GetNodeIndex(to), length);
        _lineFromSelectedNode!.X2 = GetLeft(to) + NodeHalfSize;
        _lineFromSelectedNode.Y2 = GetTop(to) + NodeHalfSize;
        SetupListeners(_lineFromSelectedNode);
        _lines.Add(_lineFromSelectedNode, new NodePair(from, to, GetNodeIndex));

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

    private void HighlightLine(object sender, MouseEventArgs e)
    {
        (sender as Line)!.Stroke = _lineHighlightColor;
    }

    private void ResetLine(object sender, MouseEventArgs e)
    {
        ResetLines();
    }

    private void PromptLengthDialog(object sender, MouseButtonEventArgs e)
    {
        var line = (sender as Line)!;
        var nodes = _lines[line];
        var initialLength = _network.GetValue(nodes.A, nodes.B);
        var newLength = LengthDialog.Prompt(initialLength);
        _network.SetPath(nodes.A, nodes.B, newLength ?? initialLength);
        if (newLength != null) _labels[line].Content = newLength.ToString();
    }

    private int GetNodeIndex(Ellipse node)
    {
        return _nodes.IndexOf(node);
    }

    private void ResetLines()
    {
        foreach (var line in _lines.Keys)
        {
            line.Stroke = _lineDefaultColor;
        }
    }

    public void HighlightRoute(Route route)
    {
        ResetLines();
        for (var i = 0; i < route.Count - 1; i++)
        {
            var line = _lines.First(pair => 
                new NodePair(route[i], route[i + 1]) == pair.Value
                ).Key;
            line.Stroke = _routeColor;
        }
    }

    public NetworkOnCanvas GetData()
    {
        return new NetworkOnCanvas(_network, _nodes);
    }
}
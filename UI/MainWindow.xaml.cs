using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AI_labs.Files;
using AI_labs.Optimization.AntColony;
using Microsoft.Win32;

namespace AI_labs.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnFloatParameterChanged(object sender, TextCompositionEventArgs e)
        {
            var textBox = (sender as TextBox)!;
            e.Handled = !float.TryParse(textBox.Text + e.Text, out _);
        }

        private void OnIntParameterChanged(object sender, TextCompositionEventArgs e)
        {
            var textBox = (sender as TextBox)!;
            e.Handled = !int.TryParse(textBox.Text + e.Text, out _);
        }

        private void CommandManager_OnPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void StartOptimization(object sender, RoutedEventArgs e)
        {
            var aco = new AntColonyOptimization(Canvas.Network, new AntColonyOptimization.Parameters 
            {
                Alpha = float.Parse(Alpha.Text),
                Beta = float.Parse(Beta.Text),
                Evaporation = float.Parse(Evaporation.Text),
                PheromoneGain = float.Parse(PheromoneGain.Text),
                AntsNumber = int.Parse(AntsNumber.Text),
                ColonyLifetime = int.Parse(ColonyLifetime.Text),
                Home = Canvas.StartPoint
            });
            var test = Canvas.Network.Size;
            var res = aco.Optimize();
            Result.Text = $"Length: {res.Length}";
            Canvas.HighlightRoute(res);
        }

        private const string FileFilter = "Graph Network | *.gnw";

        private void NewNetwork(object sender, RoutedEventArgs e)
        {
            NewNetwork();
        }

        private void NewNetwork()
        {
            CanvasArea.Children.Remove(Canvas);
            var canvas = new NetworkCanvas()
            {
                Test = Canvas.Test + 1
            };
            CanvasArea.Children.Add(canvas);
            Canvas = canvas;
        }

        private void SaveNetwork(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = FileFilter
            };
            if (dialog.ShowDialog() == true)
            {
                Canvas.GetData().SaveToFile(dialog.FileName);
            }
        }

        private void OpenNetwork(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = FileFilter
            };
            if (dialog.ShowDialog() != true) return;
            var data = NetworkOnCanvas.ReadFromFile(dialog.FileName);
            if (data is null) return;
            NewNetwork();
            foreach (var position in data.NodePositions)
            {
                Canvas.AddNode(position);
            }
            foreach (var path in data.Paths)
            {
                Canvas.AddPath(path.NodeA, path.NodeB, path.Length);
            }
        }
    }
}
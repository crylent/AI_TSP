using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AI_labs.Files;
using AI_labs.Optimization;
using AI_labs.Optimization.AntColony;
using AI_labs.Optimization.SimulatedAnnealing;
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

        private Algorithm _algorithm = Algorithm.AntColonyOptimization;

        private void StartOptimization(object sender, RoutedEventArgs e)
        {
            OptimizationAlgorithm optimizationAlgorithm = _algorithm switch
            {
                Algorithm.AntColonyOptimization => new AntColonyOptimization(Canvas.Network,
                    new AntColonyOptimization.Parameters
                    {
                        Alpha = float.Parse(Alpha.Text),
                        Beta = float.Parse(Beta.Text),
                        Evaporation = float.Parse(Evaporation.Text),
                        PheromoneGain = float.Parse(PheromoneGain.Text),
                        AntsNumber = int.Parse(AntsNumber.Text),
                        ColonyLifetime = int.Parse(ColonyLifetime.Text),
                        Home = Canvas.StartPoint
                    }),
                Algorithm.SimulatedAnnealing => new SimulatedAnnealing(Canvas.Network,
                    new SimulatedAnnealing.Parameters
                    {
                        InitialTemperature = float.Parse(InitTemp.Text),
                        TargetTemperature = float.Parse(TargetTemp.Text),
                        CoolingRate = float.Parse(CoolingRate.Text),
                        Home = Canvas.StartPoint
                    }),
                _ => throw new ArgumentOutOfRangeException()
            };
            var res = optimizationAlgorithm.Optimize();
            (Result.Text, Result.Foreground) = Canvas.HighlightRoute(res) ?
                ($"Length: {res.Length}", Brushes.Black) : 
                ("Can't construct a cycle", Brushes.Red);
        }

        private const string FileFilter = "Graph Network | *.gnw";

        private void NewNetwork(object sender, RoutedEventArgs e)
        {
            NewNetwork();
        }

        private void NewNetwork()
        {
            CanvasArea.Children.Remove(Canvas);
            var canvas = new NetworkCanvas();
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

        private void OnAlgorithmSelected(object? sender, EventArgs eventArgs)
        {
            AntColonyParams.Visibility = Visibility.Collapsed;
            AnnealingParams.Visibility = Visibility.Collapsed;
            switch (Algorithms.SelectedIndex)
            {
                case 0: OnAntColonySelected(); break;
                case 1: OnSimulatedAnnealingSelected(); break;
            }
        }

        private void OnAntColonySelected()
        {
            AntColonyParams.Visibility = Visibility.Visible;
            _algorithm = Algorithm.AntColonyOptimization;
        }

        private void OnSimulatedAnnealingSelected()
        {
            AnnealingParams.Visibility = Visibility.Visible;
            _algorithm = Algorithm.SimulatedAnnealing;
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AI_labs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //private IOptimizationAlgorithm _algorithm = new AntColonyOptimization();
        
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
            var aco = new AntColonyOptimization(Canvas.Network)
            {
                Alpha = float.Parse(Alpha.Text),
                Beta = float.Parse(Beta.Text),
                Evaporation = float.Parse(Evaporation.Text),
                PheromoneGain = float.Parse(PheromoneGain.Text),
                AntsNumber = int.Parse(AntsNumber.Text),
                ColonyLifetime = int.Parse(ColonyLifetime.Text),
                Home = Canvas.StartPoint
            };
            var res = aco.Optimize();
            Result.Text = $"{string.Join(" ", res.Route)} ({res.RouteLength})";
        }
    }
}
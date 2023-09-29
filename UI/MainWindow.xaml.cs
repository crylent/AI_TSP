using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AI_labs.Optimization.AntColony;

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
            var res = aco.Optimize();
            Result.Text = $"Length: {res.Length}";
            Canvas.HighlightRoute(res);
        }
    }
}
using System.Windows;
using System.Windows.Controls;

namespace AI_labs;

public partial class LengthDialog : Window
{
    private LengthDialog()
    {
        InitializeComponent();
    }

    private int Value;

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var inputIsNumber = int.TryParse(LengthValue.Text, out var val);
        if (inputIsNumber && val > 0)
        {
            ConfirmButton.IsEnabled = true;
            Value = val;
        }
        else ConfirmButton.IsEnabled = false;
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    public static int? Prompt(int? initialValue = null)
    {
        var dialog = new LengthDialog();
        if (initialValue != null)
        {
            dialog.Value = (int) initialValue;
            dialog.LengthValue.Text = initialValue.ToString();
        }
        if (dialog.ShowDialog() != true) return null;
        return dialog.Value;

    }
}
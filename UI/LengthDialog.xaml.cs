using System.Windows;
using System.Windows.Controls;

namespace AI_labs.UI;

public partial class LengthDialog
{
    private LengthDialog()
    {
        InitializeComponent();
        LengthValue.Focus();
    }

    private int _value;

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var inputIsNumber = int.TryParse(LengthValue.Text, out var val);
        if (inputIsNumber && val > 0)
        {
            ConfirmButton.IsEnabled = true;
            _value = val;
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
            dialog._value = (int) initialValue;
            dialog.LengthValue.Text = initialValue.ToString();
        }
        else
        {
            dialog.ConfirmButton.IsEnabled = false;
        }
        if (dialog.ShowDialog() != true) return null;
        return dialog._value;

    }
}
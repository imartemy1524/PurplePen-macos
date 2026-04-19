// TeamVariationsDialog.axaml.cs
//
// Code-behind for the relay team variations dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Relay team variations dialog.
    /// </summary>
    public partial class TeamVariationsDialog : Window
    {
        public TeamVariationsDialog()
        {
            InitializeComponent();
            Opened += (s, e) => buttonExport.Focus();
        }

        private void ButtonClose_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }
    }
}

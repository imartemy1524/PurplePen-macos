// SettingsDialog.axaml.cs
//
// Code-behind for the application settings dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Settings dialog (language and theme).
    /// </summary>
    public partial class SettingsDialog : Window
    {
        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}

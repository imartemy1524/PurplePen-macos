// CreateKmlFiles.axaml.cs
//
// Code-behind for the KML export dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for selecting KML export options.
    /// </summary>
    public partial class CreateKmlFiles : Window
    {
        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public CreateKmlFiles()
        {
            InitializeComponent();
            Opened += (_, _) => courseListBox.Focus();
        }

        /// <summary>
        /// Accepts the dialog.
        /// </summary>
        private void CreateButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        /// <summary>
        /// Cancels the dialog.
        /// </summary>
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}

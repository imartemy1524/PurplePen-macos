// CreateImageFiles.axaml.cs
//
// Code-behind for the image export dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for selecting bitmap image export options.
    /// </summary>
    public partial class CreateImageFiles : Window
    {
        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public CreateImageFiles()
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

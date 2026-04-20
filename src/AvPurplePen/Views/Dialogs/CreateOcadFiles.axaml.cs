// CreateOcadFiles.axaml.cs
//
// Code-behind for the OCAD/OpenMapper export dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for selecting OCAD/OpenMapper export options.
    /// </summary>
    public partial class CreateOcadFiles : Window
    {
        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public CreateOcadFiles()
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

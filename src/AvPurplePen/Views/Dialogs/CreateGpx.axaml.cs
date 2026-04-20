// CreateGpx.axaml.cs
//
// Code-behind for the GPX export dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for selecting GPX export options.
    /// </summary>
    public partial class CreateGpx : Window
    {
        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public CreateGpx()
        {
            InitializeComponent();
            Opened += (_, _) => courseListBox.Focus();
        }

        /// <summary>
        /// Accepts the dialog.
        /// </summary>
        private void OkButton_Click(object? sender, RoutedEventArgs e)
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

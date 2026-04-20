// CreateRouteGadget.axaml.cs
//
// Code-behind for the RouteGadget export dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for selecting RouteGadget export options.
    /// </summary>
    public partial class CreateRouteGadget : Window
    {
        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public CreateRouteGadget()
        {
            InitializeComponent();
            Opened += (_, _) => fileNameTextBox.Focus();
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

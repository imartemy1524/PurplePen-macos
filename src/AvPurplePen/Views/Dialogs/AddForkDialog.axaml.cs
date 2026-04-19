// AddForkDialog.axaml.cs
//
// Code-behind for the add variation dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog used to choose the variation type and branch count.
    /// </summary>
    public partial class AddForkDialog : Window
    {
        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public AddForkDialog()
        {
            InitializeComponent();
            Opened += (_, _) => forkRadioButton.Focus();
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

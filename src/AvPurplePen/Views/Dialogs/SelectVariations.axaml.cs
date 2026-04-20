// SelectVariations.axaml.cs
//
// Code-behind for the choose variations dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for choosing course variation export behavior.
    /// </summary>
    public partial class SelectVariations : Window
    {
        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public SelectVariations()
        {
            InitializeComponent();
            Opened += (_, _) => variationModeCombo.Focus();
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

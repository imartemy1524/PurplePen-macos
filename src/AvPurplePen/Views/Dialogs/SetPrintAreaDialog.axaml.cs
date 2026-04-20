// SetPrintAreaDialog.axaml.cs
//
// Code-behind for the set print area dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog used to edit the current print area.
    /// </summary>
    public partial class SetPrintAreaDialog : Window
    {
        private readonly DispatcherTimer updateTimer;
        public bool DialogAccepted { get; private set; }

        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public SetPrintAreaDialog()
        {
            InitializeComponent();
            Opened += (_, _) => automaticCheckBox.Focus();

            updateTimer = new DispatcherTimer {
                Interval = System.TimeSpan.FromMilliseconds(500)
            };
            updateTimer.Tick += (_, _) => {
                if (DataContext is SetPrintAreaDialogViewModel viewModel) {
                    viewModel.DetectManualRectangleChange();
                }
            };
            updateTimer.Start();
            Closing += (_, _) => updateTimer.Stop();
        }

        /// <summary>
        /// Accepts the dialog.
        /// </summary>
        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            DialogAccepted = true;
            Close(true);
        }

        /// <summary>
        /// Cancels the dialog.
        /// </summary>
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            DialogAccepted = false;
            Close(false);
        }
    }
}

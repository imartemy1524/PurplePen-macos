// LegAssignmentsDialog.axaml.cs
//
// Code-behind for the relay fixed-branch assignments dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Relay fixed-branch assignments dialog.
    /// </summary>
    public partial class LegAssignmentsDialog : Window
    {
        public LegAssignmentsDialog()
        {
            InitializeComponent();
            Opened += (s, e) => okButton.Focus();
        }

        private async void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            LegAssignmentsDialogViewModel? vm = DataContext as LegAssignmentsDialogViewModel;
            if (vm == null)
                return;

            if (!vm.TryValidate(out string errorMessage)) {
                MessageBoxDialogViewModel errorVm = new MessageBoxDialogViewModel {
                    Message = errorMessage,
                    Icon = MessageBoxIcon.Error,
                    Buttons = MessageBoxButtons.Ok,
                    DefaultButton = MessageBoxButton.Ok
                };
                MessageBoxDialog errorDialog = new MessageBoxDialog { DataContext = errorVm };
                await errorDialog.ShowDialog(this);
                return;
            }

            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}

// AllControlsPropertiesDialog.axaml.cs
//
// Code-behind for the All Controls Properties dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for editing the printing scale and description appearance
    /// used for all controls.
    /// </summary>
    public partial class AllControlsPropertiesDialog : Window
    {
        public AllControlsPropertiesDialog()
        {
            InitializeComponent();
            Opened += (s, e) => scaleCombo.Focus();
        }

        private async void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            AllControlsPropertiesDialogViewModel? vm = DataContext as AllControlsPropertiesDialogViewModel;
            if (vm == null)
                return;

            if (!float.TryParse(vm.PrintScaleText, out float enteredScale) || enteredScale < 100 || enteredScale > 100000) {
                MessageBoxDialogViewModel errorVm = new MessageBoxDialogViewModel {
                    Message = MiscText.BadScale,
                    Icon = MessageBoxIcon.Error,
                    Buttons = MessageBoxButtons.Ok,
                    DefaultButton = MessageBoxButton.Ok
                };
                MessageBoxDialog errorDialog = new MessageBoxDialog { DataContext = errorVm };
                await errorDialog.ShowDialog(this);
                scaleCombo.Focus();
                return;
            }

            vm.PrintScale = enteredScale;
            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}

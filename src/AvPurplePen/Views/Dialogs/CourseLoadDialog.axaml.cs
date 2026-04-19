// CourseLoadDialog.axaml.cs
//
// Code-behind for the Course Load dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for editing course load values.
    /// </summary>
    public partial class CourseLoadDialog : Window
    {
        public CourseLoadDialog()
        {
            InitializeComponent();
            Opened += (s, e) => okButton.Focus();
        }

        private async void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            CourseLoadDialogViewModel? vm = DataContext as CourseLoadDialogViewModel;
            if (vm == null)
                return;

            if (!vm.TryCommitLoads(out string errorMessage)) {
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

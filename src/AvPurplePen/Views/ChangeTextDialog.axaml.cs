// ChangeTextDialog.axaml.cs
//
// Code-behind for the Change Text dialog.

using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Code-behind for ChangeTextDialog.
    /// </summary>
    public partial class ChangeTextDialog : Window
    {
        public ChangeTextDialog()
        {
            InitializeComponent();
            DataContextChanged += ChangeTextDialog_DataContextChanged;
        }

        private void ChangeTextDialog_DataContextChanged(object? sender, System.EventArgs e)
        {
            if (DataContext is ChangeTextDialogViewModel viewModel)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChangeTextDialogViewModel.DialogResult))
            {
                if (sender is ChangeTextDialogViewModel viewModel)
                {
                    Close(viewModel.DialogResult);
                }
            }
        }
    }
}

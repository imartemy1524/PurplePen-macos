// LinePropertiesDialog.axaml.cs
//
// Code-behind for the Line Properties dialog.

using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Code-behind for LinePropertiesDialog.
    /// </summary>
    public partial class LinePropertiesDialog : Window
    {
        public LinePropertiesDialog()
        {
            InitializeComponent();
            DataContextChanged += LinePropertiesDialog_DataContextChanged;
        }

        private void LinePropertiesDialog_DataContextChanged(object? sender, System.EventArgs e)
        {
            if (DataContext is LinePropertiesDialogViewModel viewModel)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                UpdatePanelVisibility();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LinePropertiesDialogViewModel.DialogResult))
            {
                if (sender is LinePropertiesDialogViewModel viewModel)
                {
                    Close(viewModel.DialogResult);
                }
            }
            else if (e.PropertyName == nameof(LinePropertiesDialogViewModel.SelectedLineKind))
            {
                UpdatePanelVisibility();
            }
        }

        private void UpdatePanelVisibility()
        {
            if (DataContext is LinePropertiesDialogViewModel viewModel)
            {
                int lineKind = viewModel.SelectedLineKind;

                // gapSizePanel visible for Double (1) and Dashed (2)
                if (gapSizePanel != null)
                {
                    gapSizePanel.IsVisible = (lineKind == 1 || lineKind == 2);
                }

                // dashSizePanel visible only for Dashed (2)
                if (dashSizePanel != null)
                {
                    dashSizePanel.IsVisible = (lineKind == 2);
                }
            }
        }
    }
}

using Avalonia.Controls;
using PurplePen.ViewModels;
using System;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Code-behind for ChangeMapFileDialog.
    /// </summary>
    public partial class ChangeMapFileDialog : Window
    {
        public ChangeMapFileDialog()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is ChangeMapFileDialogViewModel vm)
            {
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ChangeMapFileDialogViewModel.DialogResult))
                    {
                        Close(vm.DialogResult);
                    }
                };
            }
        }
    }
}

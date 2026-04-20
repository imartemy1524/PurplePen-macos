using Avalonia.Controls;
using PurplePen.ViewModels;
using System;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Code-behind for ChangeCodesDialog.
    /// </summary>
    public partial class ChangeCodesDialog : Window
    {
        public ChangeCodesDialog()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is ChangeCodesDialogViewModel vm)
            {
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ChangeCodesDialogViewModel.DialogResult))
                    {
                        Close(vm.DialogResult);
                    }
                };
            }
        }
    }
}

using Avalonia.Controls;
using PurplePen.ViewModels;
using System;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Code-behind for AutoNumberingDialog.
    /// </summary>
    public partial class AutoNumberingDialog : Window
    {
        public AutoNumberingDialog()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is AutoNumberingDialogViewModel vm)
            {
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(AutoNumberingDialogViewModel.DialogResult))
                    {
                        Close(vm.DialogResult);
                    }
                };
            }
        }
    }
}

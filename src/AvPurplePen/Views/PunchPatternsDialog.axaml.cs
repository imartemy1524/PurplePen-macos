using Avalonia.Controls;
using PurplePen.ViewModels;
using System;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Code-behind for PunchPatternsDialog.
    /// </summary>
    public partial class PunchPatternsDialog : Window
    {
        public PunchPatternsDialog()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is PunchPatternsDialogViewModel vm)
            {
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(PunchPatternsDialogViewModel.DialogResult))
                    {
                        Close(vm.DialogResult);
                    }
                };
            }
        }
    }
}

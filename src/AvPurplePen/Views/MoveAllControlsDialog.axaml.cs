using Avalonia.Controls;
using PurplePen.ViewModels;
using System;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Code-behind for MoveAllControlsDialog.
    /// </summary>
    public partial class MoveAllControlsDialog : Window
    {
        public MoveAllControlsDialog()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is MoveAllControlsDialogViewModel vm)
            {
                // Set initial radio button state based on SelectedActionIndex
                UpdateRadioButtonsFromViewModel(vm);

                // Wire up radio button changes to update ViewModel
                var radioMove = this.FindControl<RadioButton>("radioMove");
                var radioMoveScale = this.FindControl<RadioButton>("radioMoveScale");
                var radioMoveRotate = this.FindControl<RadioButton>("radioMoveRotate");
                var radioMoveRotateScale = this.FindControl<RadioButton>("radioMoveRotateScale");

                if (radioMove != null)
                    radioMove.IsCheckedChanged += (s, e) => { if (radioMove.IsChecked == true) vm.SelectedActionIndex = 0; };
                if (radioMoveScale != null)
                    radioMoveScale.IsCheckedChanged += (s, e) => { if (radioMoveScale.IsChecked == true) vm.SelectedActionIndex = 1; };
                if (radioMoveRotate != null)
                    radioMoveRotate.IsCheckedChanged += (s, e) => { if (radioMoveRotate.IsChecked == true) vm.SelectedActionIndex = 2; };
                if (radioMoveRotateScale != null)
                    radioMoveRotateScale.IsCheckedChanged += (s, e) => { if (radioMoveRotateScale.IsChecked == true) vm.SelectedActionIndex = 3; };

                // Handle dialog result changes
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(MoveAllControlsDialogViewModel.DialogResult))
                    {
                        Close(vm.DialogResult);
                    }
                };
            }
        }

        private void UpdateRadioButtonsFromViewModel(MoveAllControlsDialogViewModel vm)
        {
            var radioMove = this.FindControl<RadioButton>("radioMove");
            var radioMoveScale = this.FindControl<RadioButton>("radioMoveScale");
            var radioMoveRotate = this.FindControl<RadioButton>("radioMoveRotate");
            var radioMoveRotateScale = this.FindControl<RadioButton>("radioMoveRotateScale");

            if (radioMove != null) radioMove.IsChecked = vm.SelectedActionIndex == 0;
            if (radioMoveScale != null) radioMoveScale.IsChecked = vm.SelectedActionIndex == 1;
            if (radioMoveRotate != null) radioMoveRotate.IsChecked = vm.SelectedActionIndex == 2;
            if (radioMoveRotateScale != null) radioMoveRotateScale.IsChecked = vm.SelectedActionIndex == 3;
        }
    }
}

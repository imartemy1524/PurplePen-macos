using Avalonia.Controls;
using PurplePen.ViewModels;
using System;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Code-behind for CustomizeCourseAppearanceDialog.
    /// </summary>
    public partial class CustomizeCourseAppearanceDialog : Window
    {
        public CustomizeCourseAppearanceDialog()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is CustomizeCourseAppearanceDialogViewModel vm)
            {
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CustomizeCourseAppearanceDialogViewModel.DialogResult))
                    {
                        Close(vm.DialogResult);
                    }
                };
            }
        }
    }
}

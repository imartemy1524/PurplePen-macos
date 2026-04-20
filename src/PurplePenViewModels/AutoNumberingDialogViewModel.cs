using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Auto Numbering dialog.
    /// Allows configuration of automatic control numbering.
    /// </summary>
    public partial class AutoNumberingDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int firstCode = 101;

        [ObservableProperty]
        private bool disallowInvertibleCodes = false;

        [ObservableProperty]
        private bool renumberExisting = false;

        [ObservableProperty]
        private bool dialogResult = false;

        public AutoNumberingDialogViewModel()
        {
        }

        public void Initialize(int firstCode, bool disallowInvertibleCodes)
        {
            FirstCode = firstCode;
            DisallowInvertibleCodes = disallowInvertibleCodes;
            RenumberExisting = false;
        }

        [RelayCommand]
        private void Ok()
        {
            DialogResult = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false;
        }
    }
}

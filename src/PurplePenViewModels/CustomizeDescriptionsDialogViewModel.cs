using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Customize Descriptions dialog.
    /// Allows customization of symbol text for course descriptions.
    /// </summary>
    public partial class CustomizeDescriptionsDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string currentLanguage = "English";

        [ObservableProperty]
        private ObservableCollection<string> availableLanguages = new();

        [ObservableProperty]
        private bool useAsDefault = false;

        [ObservableProperty]
        private bool dialogResult = false;

        public CustomizeDescriptionsDialogViewModel()
        {
            AvailableLanguages.Add("English");
            AvailableLanguages.Add("French");
            AvailableLanguages.Add("German");
            AvailableLanguages.Add("Spanish");
            AvailableLanguages.Add("Italian");
            AvailableLanguages.Add("Dutch");
            AvailableLanguages.Add("Swedish");
            AvailableLanguages.Add("Norwegian");
            AvailableLanguages.Add("Danish");
            AvailableLanguages.Add("Russian");
            AvailableLanguages.Add("Czech");
            AvailableLanguages.Add("Chinese");
            AvailableLanguages.Add("Japanese");
        }

        public void Initialize(string langId)
        {
            CurrentLanguage = langId ?? "English";
            UseAsDefault = false;
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

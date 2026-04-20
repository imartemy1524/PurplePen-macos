// SettingsDialogViewModel.cs
//
// ViewModel for application settings (currently language and theme).

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings dialog.
    /// </summary>
    public partial class SettingsDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Available language options.
        /// </summary>
        public ObservableCollection<LanguageItem> AvailableLanguages { get; }

        /// <summary>
        /// Selected language.
        /// </summary>
        [ObservableProperty]
        private LanguageItem? selectedLanguage;

        /// <summary>
        /// Selected theme index: 0 = System, 1 = Light, 2 = Dark.
        /// </summary>
        [ObservableProperty]
        private int selectedThemeIndex;

        /// <summary>
        /// Constructor for the designer.
        /// </summary>
        public SettingsDialogViewModel()
            : this("en", "System")
        {
        }

        /// <summary>
        /// Creates settings view model with preselected language and theme.
        /// </summary>
        public SettingsDialogViewModel(string currentLanguageCode, string currentThemeCode)
        {
            AvailableLanguages = SwitchLanguageDialogViewModel.CreateDefaultLanguages();

            foreach (LanguageItem language in AvailableLanguages) {
                if (string.Equals(language.Code, currentLanguageCode, System.StringComparison.OrdinalIgnoreCase)) {
                    SelectedLanguage = language;
                    break;
                }
            }

            if (SelectedLanguage == null && AvailableLanguages.Count > 0) {
                SelectedLanguage = AvailableLanguages[0];
            }

            if (string.Equals(currentThemeCode, "Light", System.StringComparison.OrdinalIgnoreCase)) {
                SelectedThemeIndex = 1;
            }
            else if (string.Equals(currentThemeCode, "Dark", System.StringComparison.OrdinalIgnoreCase)) {
                SelectedThemeIndex = 2;
            }
            else {
                SelectedThemeIndex = 0;
            }
        }

        /// <summary>
        /// Converts the selected theme index to theme code.
        /// </summary>
        public string GetSelectedThemeCode()
        {
            return SelectedThemeIndex switch {
                1 => "Light",
                2 => "Dark",
                _ => "System"
            };
        }
    }
}

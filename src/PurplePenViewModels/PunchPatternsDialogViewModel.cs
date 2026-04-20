using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Punch Patterns dialog.
    /// Allows viewing and managing punch patterns for punchcards.
    /// </summary>
    public partial class PunchPatternsDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<string> patterns = new();

        [ObservableProperty]
        private string selectedPattern = "";

        [ObservableProperty]
        private bool dialogResult = false;

        public PunchPatternsDialogViewModel()
        {
        }

        public void Initialize(Dictionary<string, PunchPattern> allPatterns)
        {
            Patterns.Clear();
            foreach (var patternName in allPatterns.Keys.OrderBy(k => k))
            {
                Patterns.Add(patternName);
            }

            if (Patterns.Count > 0)
            {
                SelectedPattern = Patterns[0];
            }
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

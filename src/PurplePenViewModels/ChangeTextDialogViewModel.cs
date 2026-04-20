// ChangeTextDialogViewModel.cs
//
// ViewModel for the Change Text dialog.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PurplePen.Graphics2D;
using System;
using System.Collections.ObjectModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Change Text dialog.
    /// </summary>
    public partial class ChangeTextDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string userText = "";

        [ObservableProperty]
        private string selectedFontName = "Arial";

        [ObservableProperty]
        private ObservableCollection<string> availableFonts = new();

        [ObservableProperty]
        private bool fontBold;

        [ObservableProperty]
        private bool fontItalic;

        [ObservableProperty]
        private float fontSize = 10f;

        [ObservableProperty]
        private bool fontSizeAutomatic = true;

        [ObservableProperty]
        private SpecialColor fontColor = SpecialColor.Black;

        [ObservableProperty]
        private string explanation = "";

        [ObservableProperty]
        private bool allowSpecialTextInsert;

        [ObservableProperty]
        private bool dialogResult = false;

        public TextEffects TextEffects => Util.GetTextEffects(FontBold, FontItalic);

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

        public void Initialize(string title, string explanation, bool allowSpecialTextInsert, SpecialColor purpleColor, Func<string, string> textExpander)
        {
            Explanation = explanation;
            AllowSpecialTextInsert = allowSpecialTextInsert;
            FontColor = purpleColor;

            // Populate available fonts
            AvailableFonts.Clear();
            AvailableFonts.Add("Arial");
            AvailableFonts.Add("Helvetica");
            AvailableFonts.Add("Times New Roman");
            AvailableFonts.Add("Courier");
            AvailableFonts.Add("Verdana");
            AvailableFonts.Add("Georgia");
            AvailableFonts.Add("Trebuchet MS");

            SelectedFontName = "Arial";
        }
    }
}

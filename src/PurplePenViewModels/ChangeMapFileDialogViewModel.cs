using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Change Map File dialog.
    /// Allows changing the map file associated with an event.
    /// </summary>
    public partial class ChangeMapFileDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string mapFileName = "";

        [ObservableProperty]
        private float mapScale = 1.0f;

        [ObservableProperty]
        private float mapDpi = 96.0f;

        [ObservableProperty]
        private int selectedMapType = 1; // 0=OCAD, 1=Bitmap, 2=PDF

        [ObservableProperty]
        private bool dialogResult = false;

        [ObservableProperty]
        private string mapTypeName = "Bitmap";

        [ObservableProperty]
        private bool showDpiControl = true;

        public ChangeMapFileDialogViewModel()
        {
        }

        public void Initialize(string currentMapFile, int mapType, float scale, float dpi)
        {
            MapFileName = currentMapFile;
            SelectedMapType = mapType;
            MapScale = scale;
            MapDpi = dpi;
            UpdateMapTypeName();
        }

        private void UpdateMapTypeName()
        {
            MapTypeName = SelectedMapType switch
            {
                0 => "OCAD",
                1 => "Bitmap",
                2 => "PDF",
                _ => "Unknown"
            };
            // Only show DPI for bitmap maps
            ShowDpiControl = SelectedMapType == 1;
        }

        [RelayCommand]
        private void BrowseFile()
        {
            // This will be handled by the view's file picker
        }

        [RelayCommand]
        private void Ok()
        {
            if (!string.IsNullOrEmpty(MapFileName))
            {
                DialogResult = true;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false;
        }
    }
}

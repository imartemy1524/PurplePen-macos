// CreateImageFilesViewModel.cs
//
// ViewModel for the bitmap image export dialog.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for selecting courses and output options when creating bitmap image files.
    /// </summary>
    public partial class CreateImageFilesViewModel : ViewModelBase
    {
        private BitmapCreationSettings settings = new BitmapCreationSettings();

        /// <summary>
        /// Shared course selector.
        /// </summary>
        public CourseSelectionViewModel CourseSelection { get; } = new CourseSelectionViewModel();

        /// <summary>
        /// Shared output folder selector.
        /// </summary>
        public OutputFolderSelectionViewModel OutputFolder { get; } = new OutputFolderSelectionViewModel();

        /// <summary>
        /// Bitmap file format options.
        /// </summary>
        public ObservableCollection<string> FileFormatOptions { get; } = new ObservableCollection<string>();

        /// <summary>
        /// DPI options.
        /// </summary>
        public ObservableCollection<string> DpiOptions { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Yes/no world file options.
        /// </summary>
        public ObservableCollection<string> WorldFileOptions { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Color model options.
        /// </summary>
        public ObservableCollection<string> ColorModelOptions { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Base map inclusion options.
        /// </summary>
        public ObservableCollection<string> PrintBaseMapOptions { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Optional filename prefix.
        /// </summary>
        [ObservableProperty]
        private string filePrefix = "";

        /// <summary>
        /// Selected bitmap file format index.
        /// </summary>
        [ObservableProperty]
        private int selectedFileFormatIndex;

        /// <summary>
        /// Editable DPI text.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOkEnabled))]
        private string dpiText = "200";

        /// <summary>
        /// Selected world file option index.
        /// </summary>
        [ObservableProperty]
        private int worldFileIndex;

        /// <summary>
        /// True when world file creation is available.
        /// </summary>
        [ObservableProperty]
        private bool worldFileEnabled = true;

        /// <summary>
        /// Selected color model index.
        /// </summary>
        [ObservableProperty]
        private int colorModelIndex = 1;

        /// <summary>
        /// Selected base map inclusion index.
        /// </summary>
        [ObservableProperty]
        private int printBaseMapIndex;

        /// <summary>
        /// True when the dialog has enough information to create files.
        /// </summary>
        public bool IsOkEnabled {
            get {
                float dpi;
                return CourseSelection.SelectedCourseIds().Length > 0 &&
                       float.TryParse(DpiText, NumberStyles.Float, CultureInfo.CurrentCulture, out dpi) &&
                       dpi > 0 &&
                       OutputFolder.IsValid;
            }
        }

        /// <summary>
        /// Parameterless constructor for the designer.
        /// </summary>
        public CreateImageFilesViewModel()
        {
            InitializeOptionLists();
            CourseSelection.SelectionChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
            OutputFolder.SelectionChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Initializes the dialog from the current event and existing settings.
        /// </summary>
        /// <param name="eventDB">The current event database.</param>
        /// <param name="settings">The existing settings to edit.</param>
        /// <param name="worldFileEnabled">Whether world file creation is available.</param>
        public void Initialize(EventDB eventDB, BitmapCreationSettings settings, bool worldFileEnabled)
        {
            this.settings = settings.Clone();
            WorldFileEnabled = worldFileEnabled;
            if (!WorldFileEnabled) {
                this.settings.WorldFile = false;
            }

            CourseSelection.Initialize(eventDB, this.settings.CourseIds, this.settings.AllCourses, this.settings.VariationChoicesPerCourse, true, true);

            OutputFolder.Initialize(this.settings.fileDirectory, this.settings.mapDirectory, this.settings.outputDirectory);
            FilePrefix = this.settings.filePrefix ?? "";
            SelectedFileFormatIndex = BitmapKindToIndex(this.settings.ExportedBitmapKind);
            DpiText = this.settings.Dpi > 0 ? this.settings.Dpi.ToString(CultureInfo.CurrentCulture) : "200";
            WorldFileIndex = this.settings.WorldFile ? 1 : 0;
            ColorModelIndex = this.settings.ColorModel == ColorModel.CMYK ? 1 : 0;
            PrintBaseMapIndex = this.settings.DontPrintBaseMap ? 1 : 0;
            OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Builds export settings from the current dialog values.
        /// </summary>
        /// <returns>The settings represented by the dialog.</returns>
        public BitmapCreationSettings BuildSettings()
        {
            BitmapCreationSettings result = settings.Clone();
            result.CourseIds = CourseSelection.SelectedCourseIds();
            result.AllCourses = CourseSelection.AllCoursesSelected();
            result.fileDirectory = OutputFolder.IsCoursesDirectory;
            result.mapDirectory = OutputFolder.IsMapDirectory;
            result.outputDirectory = OutputFolder.OutputDirectory;
            result.filePrefix = FilePrefix;
            result.ExportedBitmapKind = IndexToBitmapKind(SelectedFileFormatIndex);

            float dpi;
            if (float.TryParse(DpiText, NumberStyles.Float, CultureInfo.CurrentCulture, out dpi) && dpi > 0) {
                result.Dpi = dpi;
            }
            else {
                result.Dpi = 200;
            }

            result.WorldFile = WorldFileEnabled && WorldFileIndex == 1;
            result.ColorModel = ColorModelIndex == 1 ? ColorModel.CMYK : ColorModel.RGB;
            result.DontPrintBaseMap = PrintBaseMapIndex == 1;
            result.VariationChoicesPerCourse = CourseSelection.VariationChoicesPerCourse();
            return result;
        }

        /// <summary>
        /// Populates static option lists.
        /// </summary>
        private void InitializeOptionLists()
        {
            if (FileFormatOptions.Count > 0) { return; }

            FileFormatOptions.Add("PNG");
            FileFormatOptions.Add("JPEG");
            FileFormatOptions.Add("GIF");

            DpiOptions.Add("100");
            DpiOptions.Add("150");
            DpiOptions.Add("200");
            DpiOptions.Add("300");
            DpiOptions.Add("400");
            DpiOptions.Add("500");
            DpiOptions.Add("600");

            WorldFileOptions.Add("No");
            WorldFileOptions.Add("Yes");

            ColorModelOptions.Add("RGB");
            ColorModelOptions.Add("CMYK");

            PrintBaseMapOptions.Add("Course and map");
            PrintBaseMapOptions.Add("Course only");
        }

        /// <summary>
        /// Converts a bitmap kind to the matching combo-box index.
        /// </summary>
        /// <param name="kind">The bitmap kind.</param>
        /// <returns>The combo-box index.</returns>
        private static int BitmapKindToIndex(BitmapCreationSettings.BitmapKind kind)
        {
            switch (kind) {
                case BitmapCreationSettings.BitmapKind.Png:
                    return 0;
                case BitmapCreationSettings.BitmapKind.Jpeg:
                    return 1;
                case BitmapCreationSettings.BitmapKind.Gif:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Converts a combo-box index to the matching bitmap kind.
        /// </summary>
        /// <param name="index">The combo-box index.</param>
        /// <returns>The bitmap kind.</returns>
        private static BitmapCreationSettings.BitmapKind IndexToBitmapKind(int index)
        {
            switch (index) {
                case 1:
                    return BitmapCreationSettings.BitmapKind.Jpeg;
                case 2:
                    return BitmapCreationSettings.BitmapKind.Gif;
                default:
                    return BitmapCreationSettings.BitmapKind.Png;
            }
        }
    }
}

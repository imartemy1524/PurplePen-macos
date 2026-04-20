// CreatePdfCoursesViewModel.cs
//
// ViewModel for the course PDF export dialog.

using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for selecting courses and output settings for course PDF export.
    /// </summary>
    public partial class CreatePdfCoursesViewModel : ViewModelBase
    {
        /// <summary>
        /// Shared course selector.
        /// </summary>
        public CourseSelectionViewModel CourseSelection { get; } = new CourseSelectionViewModel();

        /// <summary>
        /// Shared output folder selector.
        /// </summary>
        public OutputFolderSelectionViewModel OutputFolder { get; } = new OutputFolderSelectionViewModel();

        /// <summary>
        /// Optional filename prefix.
        /// </summary>
        [ObservableProperty]
        private string filePrefix = "";

        /// <summary>
        /// File creation mode: 0 = one file for all courses, 1 = one per course, 2 = one per course part/variation.
        /// </summary>
        [ObservableProperty]
        private int fileCreationIndex = 1;

        /// <summary>
        /// Color model index: 0 = RGB, 1 = CMYK.
        /// </summary>
        [ObservableProperty]
        private int colorModelIndex = 1;

        /// <summary>
        /// Include the base map in the PDF output.
        /// </summary>
        [ObservableProperty]
        private int printBaseMapIndex = 0;

        /// <summary>
        /// Crop large print areas to a single page, or split over multiple pages.
        /// </summary>
        [ObservableProperty]
        private int multiPageIndex = 0;

        /// <summary>
        /// Print map exchanges on the same map.
        /// </summary>
        [ObservableProperty]
        private bool mergeParts;

        /// <summary>
        /// Whether the crop mode can be changed.
        /// </summary>
        [ObservableProperty]
        private bool canChangeCropping = true;

        /// <summary>
        /// True when the current selections are valid.
        /// </summary>
        public bool IsOkEnabled => CourseSelection.SelectedCourseIds().Length > 0 && OutputFolder.IsValid;

        /// <summary>
        /// The settings currently produced by the dialog.
        /// </summary>
        public CoursePdfSettings PdfSettings { get; private set; } = new CoursePdfSettings();

        /// <summary>
        /// Parameterless constructor for the designer.
        /// </summary>
        public CreatePdfCoursesViewModel()
        {
            CourseSelection.SelectionChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
            OutputFolder.SelectionChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Initialize the dialog from the current event database.
        /// </summary>
        /// <param name="eventDB">The current event database.</param>
        /// <param name="anyMultipart">Whether any course has multiple parts.</param>
        /// <param name="settings">Optional preexisting settings.</param>
        public void Initialize(EventDB eventDB, bool anyMultipart, CoursePdfSettings? settings = null)
        {
            CoursePdfSettings useSettings = settings != null ? settings.Clone() : new CoursePdfSettings();

            PdfSettings = useSettings;
            FileCreationIndex = (int) useSettings.FileCreation;
            ColorModelIndex = (useSettings.ColorModel == ColorModel.RGB) ? 0 : 1;
            PrintBaseMapIndex = useSettings.DontPrintBaseMap ? 1 : 0;
            MultiPageIndex = useSettings.CropLargePrintArea ? 0 : 1;
            MergeParts = useSettings.PrintMapExchangesOnOneMap;

            CourseSelection.Initialize(eventDB, useSettings.CourseIds, useSettings.AllCourses, useSettings.VariationChoicesPerCourse, true, true);
            OutputFolder.Initialize(useSettings.fileDirectory, useSettings.mapDirectory, useSettings.outputDirectory);
            FilePrefix = useSettings.filePrefix ?? "";
            CanChangeCropping = true;
            NormalizeOutputDirectory(eventDB);

            if (!anyMultipart) {
                MergeParts = false;
            }
        }

        /// <summary>
        /// Gets the settings represented by the current dialog values.
        /// </summary>
        public CoursePdfSettings BuildSettings()
        {
            CoursePdfSettings settings = PdfSettings != null ? PdfSettings.Clone() : new CoursePdfSettings();
            settings.CourseIds = CourseSelection.SelectedCourseIds();
            settings.AllCourses = CourseSelection.AllCoursesSelected();
            settings.VariationChoicesPerCourse = CourseSelection.VariationChoicesPerCourse();
            settings.FileCreation = (CoursePdfSettings.PdfFileCreation) FileCreationIndex;
            settings.ColorModel = (ColorModel) (ColorModelIndex + 1);
            settings.DontPrintBaseMap = (PrintBaseMapIndex == 1);
            settings.CropLargePrintArea = (MultiPageIndex == 0);
            settings.PrintMapExchangesOnOneMap = MergeParts;
            settings.fileDirectory = OutputFolder.IsCoursesDirectory;
            settings.mapDirectory = OutputFolder.IsMapDirectory;
            settings.outputDirectory = OutputFolder.OutputDirectory;
            settings.filePrefix = FilePrefix;
            return settings;
        }

        /// <summary>
        /// Keeps the OK button state in sync.
        /// </summary>
        partial void OnFilePrefixChanged(string value)
        {
            OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Fills in a default output directory when possible.
        /// </summary>
        private void NormalizeOutputDirectory(EventDB eventDB)
        {
            if (string.IsNullOrWhiteSpace(OutputFolder.OutputDirectory)) {
                string? mapFileName = eventDB.GetEvent().mapFileName;
                if (!string.IsNullOrWhiteSpace(mapFileName)) {
                    OutputFolder.OutputDirectory = Path.GetDirectoryName(mapFileName) ?? "";
                }
            }
        }
    }
}

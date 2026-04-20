// CreateKmlFilesViewModel.cs
//
// ViewModel for the KML export dialog.

using CommunityToolkit.Mvvm.ComponentModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for selecting courses and output options when creating KML files.
    /// </summary>
    public partial class CreateKmlFilesViewModel : ViewModelBase
    {
        private ExportKmlSettings settings = new ExportKmlSettings();

        /// <summary>
        /// Shared selectable course rows and variation chooser.
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
        /// File creation mode: 0 = one file for all courses, 1 = one per course.
        /// </summary>
        [ObservableProperty]
        private int fileCreationIndex = 1;

        /// <summary>
        /// True when the dialog has enough information to create files.
        /// </summary>
        public bool IsOkEnabled => CourseSelection.SelectedCourseIds().Length > 0 &&
                                   OutputFolder.IsValid;

        /// <summary>
        /// Parameterless constructor for the designer.
        /// </summary>
        public CreateKmlFilesViewModel()
        {
            CourseSelection.SelectionChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
            OutputFolder.SelectionChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Initializes the dialog from the current event and existing settings.
        /// </summary>
        /// <param name="eventDB">The current event database.</param>
        /// <param name="settings">The existing settings to edit.</param>
        public void Initialize(EventDB eventDB, ExportKmlSettings settings)
        {
            this.settings = settings.Clone();
            CourseSelection.Initialize(eventDB, this.settings.CourseIds, this.settings.AllCourses, this.settings.VariationChoicesPerCourse, true, true);

            OutputFolder.Initialize(this.settings.fileDirectory, this.settings.mapDirectory, this.settings.outputDirectory);
            FilePrefix = this.settings.filePrefix ?? "";
            FileCreationIndex = this.settings.FileCreation == ExportKmlSettings.KmlFileCreation.SingleFile ? 0 : 1;
            OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Builds export settings from the current dialog values.
        /// </summary>
        /// <returns>The settings represented by the dialog.</returns>
        public ExportKmlSettings BuildSettings()
        {
            ExportKmlSettings result = settings.Clone();
            result.CourseIds = CourseSelection.SelectedCourseIds();
            result.AllCourses = CourseSelection.AllCoursesSelected();
            result.fileDirectory = OutputFolder.IsCoursesDirectory;
            result.mapDirectory = OutputFolder.IsMapDirectory;
            result.outputDirectory = OutputFolder.OutputDirectory;
            result.filePrefix = FilePrefix;
            result.FileCreation = FileCreationIndex == 0 ? ExportKmlSettings.KmlFileCreation.SingleFile : ExportKmlSettings.KmlFileCreation.FilePerCourse;
            result.VariationChoicesPerCourse = CourseSelection.VariationChoicesPerCourse();
            return result;
        }
    }
}

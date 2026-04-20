// CreateGpxViewModel.cs
//
// ViewModel for the GPX export dialog.

using CommunityToolkit.Mvvm.ComponentModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for selecting courses and waypoint options when creating a GPX file.
    /// </summary>
    public partial class CreateGpxViewModel : ViewModelBase
    {
        private GpxCreationSettings settings = new GpxCreationSettings();

        /// <summary>
        /// Shared selectable course rows.
        /// </summary>
        public CourseSelectionViewModel CourseSelection { get; } = new CourseSelectionViewModel();

        /// <summary>
        /// Prefix to add to exported waypoint names.
        /// </summary>
        [ObservableProperty]
        private string codePrefix = "";

        /// <summary>
        /// True when at least one course row is selected.
        /// </summary>
        public bool IsOkEnabled => CourseSelection.SelectedCourseIds().Length > 0;

        /// <summary>
        /// Parameterless constructor for the designer.
        /// </summary>
        public CreateGpxViewModel()
        {
            CourseSelection.SelectionChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Initializes the dialog from the current event and existing settings.
        /// </summary>
        /// <param name="eventDB">The current event database.</param>
        /// <param name="settings">The existing settings to edit.</param>
        public void Initialize(EventDB eventDB, GpxCreationSettings settings)
        {
            this.settings = settings.Clone();
            CourseSelection.Initialize(eventDB, this.settings.CourseIds, this.settings.AllCourses, null, true, false);
            CodePrefix = this.settings.CodePrefix ?? "";
            OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Builds export settings from the current dialog values.
        /// </summary>
        /// <returns>The settings represented by the dialog.</returns>
        public GpxCreationSettings BuildSettings()
        {
            GpxCreationSettings result = settings.Clone();
            result.CourseIds = CourseSelection.SelectedCourseIds();
            result.AllCourses = CourseSelection.AllCoursesSelected();
            result.CodePrefix = CodePrefix;
            return result;
        }
    }
}

// CreateRouteGadgetViewModel.cs
//
// ViewModel for the RouteGadget export dialog.

using CommunityToolkit.Mvvm.ComponentModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for selecting output options when creating RouteGadget files.
    /// </summary>
    public partial class CreateRouteGadgetViewModel : ViewModelBase
    {
        private RouteGadgetCreationSettings settings = new RouteGadgetCreationSettings();

        /// <summary>
        /// Shared output folder selector.
        /// </summary>
        public OutputFolderSelectionViewModel OutputFolder { get; } = new OutputFolderSelectionViewModel();

        /// <summary>
        /// Base file name without .xml or .gif.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOkEnabled))]
        private string fileBaseName = "";

        /// <summary>
        /// IOF XML version selector: 0 = 2.0.3, 1 = 3.0.
        /// </summary>
        [ObservableProperty]
        private int xmlVersionIndex = 1;

        /// <summary>
        /// True when the dialog has enough information to create files.
        /// </summary>
        public bool IsOkEnabled => !string.IsNullOrWhiteSpace(FileBaseName) &&
                                   OutputFolder.IsValid;

        /// <summary>
        /// Parameterless constructor for the designer.
        /// </summary>
        public CreateRouteGadgetViewModel()
        {
            OutputFolder.SelectionChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Initializes the dialog from existing settings.
        /// </summary>
        /// <param name="settings">The existing settings to edit.</param>
        public void Initialize(RouteGadgetCreationSettings settings)
        {
            this.settings = settings.Clone();
            OutputFolder.Initialize(this.settings.fileDirectory, this.settings.mapDirectory, this.settings.outputDirectory);
            FileBaseName = this.settings.fileBaseName ?? "";
            XmlVersionIndex = this.settings.xmlVersion == 2 ? 0 : 1;
            OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Builds export settings from the current dialog values.
        /// </summary>
        /// <returns>The settings represented by the dialog.</returns>
        public RouteGadgetCreationSettings BuildSettings()
        {
            RouteGadgetCreationSettings result = settings.Clone();
            result.fileDirectory = OutputFolder.IsCoursesDirectory;
            result.mapDirectory = OutputFolder.IsMapDirectory;
            result.outputDirectory = OutputFolder.OutputDirectory;
            result.fileBaseName = FileBaseName;
            result.xmlVersion = XmlVersionIndex == 0 ? 2 : 3;
            return result;
        }
    }
}

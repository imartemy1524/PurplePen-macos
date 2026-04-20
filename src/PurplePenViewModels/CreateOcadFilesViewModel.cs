// CreateOcadFilesViewModel.cs
//
// ViewModel for the OCAD/OpenMapper file export dialog.

using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for selecting courses and output options when creating OCAD/OpenMapper files.
    /// </summary>
    public partial class CreateOcadFilesViewModel : ViewModelBase
    {
        private readonly List<MapFileFormat> fileFormatDescriptors = new List<MapFileFormat>();
        private OcadCreationSettings settings = new OcadCreationSettings();

        /// <summary>
        /// Shared course selector.
        /// </summary>
        public CourseSelectionViewModel CourseSelection { get; } = new CourseSelectionViewModel();

        /// <summary>
        /// Shared output folder selector.
        /// </summary>
        public OutputFolderSelectionViewModel OutputFolder { get; } = new OutputFolderSelectionViewModel();

        /// <summary>
        /// Selectable output file format labels.
        /// </summary>
        public ObservableCollection<string> FileFormatOptions { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Optional filename prefix.
        /// </summary>
        [ObservableProperty]
        private string filePrefix = "";

        /// <summary>
        /// Selected file format index.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOkEnabled))]
        private int selectedFileFormatIndex;

        /// <summary>
        /// True when the dialog has enough information to create files.
        /// </summary>
        public bool IsOkEnabled => CourseSelection.SelectedCourseIds().Length > 0 &&
                                   FileFormatOptions.Count > 0 &&
                                   SelectedFileFormatIndex >= 0 &&
                                   SelectedFileFormatIndex < FileFormatOptions.Count &&
                                   OutputFolder.IsValid;

        /// <summary>
        /// Parameterless constructor for the designer.
        /// </summary>
        public CreateOcadFilesViewModel()
        {
            CourseSelection.SelectionChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
            OutputFolder.SelectionChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Initializes the dialog from the current event and existing settings.
        /// </summary>
        /// <param name="eventDB">The current event database.</param>
        /// <param name="restrictToKind">Optional format kind restriction.</param>
        /// <param name="settings">The existing settings to edit.</param>
        public void Initialize(EventDB eventDB, MapFileFormatKind restrictToKind, OcadCreationSettings settings)
        {
            this.settings = settings.Clone();

            CourseSelection.Initialize(eventDB, this.settings.CourseIds, this.settings.AllCourses, this.settings.VariationChoicesPerCourse, true, true);
            InitializeFileFormats(restrictToKind, this.settings.fileFormat);

            OutputFolder.Initialize(this.settings.fileDirectory, this.settings.mapDirectory, this.settings.outputDirectory);
            FilePrefix = this.settings.filePrefix ?? "";
            OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Builds export settings from the current dialog values.
        /// </summary>
        /// <returns>The settings represented by the dialog.</returns>
        public OcadCreationSettings BuildSettings()
        {
            OcadCreationSettings result = settings.Clone();
            result.CourseIds = CourseSelection.SelectedCourseIds();
            result.AllCourses = CourseSelection.AllCoursesSelected();
            result.fileDirectory = OutputFolder.IsCoursesDirectory;
            result.mapDirectory = OutputFolder.IsMapDirectory;
            result.outputDirectory = OutputFolder.OutputDirectory;
            result.filePrefix = FilePrefix;
            result.fileFormat = fileFormatDescriptors[Math.Clamp(SelectedFileFormatIndex, 0, fileFormatDescriptors.Count - 1)];
            result.VariationChoicesPerCourse = CourseSelection.VariationChoicesPerCourse();
            return result;
        }

        /// <summary>
        /// Populates selectable output formats, optionally restricting to one map format kind.
        /// </summary>
        /// <param name="restrictToKind">The format kind restriction, or None for no restriction.</param>
        /// <param name="selectedFormat">The format to select initially.</param>
        private void InitializeFileFormats(MapFileFormatKind restrictToKind, MapFileFormat selectedFormat)
        {
            FileFormatOptions.Clear();
            fileFormatDescriptors.Clear();

            AddFileFormat(restrictToKind, MiscText.OCAD + " 6", new MapFileFormat(MapFileFormatKind.OCAD, 6));
            AddFileFormat(restrictToKind, MiscText.OCAD + " 7", new MapFileFormat(MapFileFormatKind.OCAD, 7));
            AddFileFormat(restrictToKind, MiscText.OCAD + " 8", new MapFileFormat(MapFileFormatKind.OCAD, 8));
            AddFileFormat(restrictToKind, MiscText.OCAD + " 9", new MapFileFormat(MapFileFormatKind.OCAD, 9));
            AddFileFormat(restrictToKind, MiscText.OCAD + " 10", new MapFileFormat(MapFileFormatKind.OCAD, 10));
            AddFileFormat(restrictToKind, MiscText.OCAD + " 11", new MapFileFormat(MapFileFormatKind.OCAD, 11));
            AddFileFormat(restrictToKind, MiscText.OCAD + " 12", new MapFileFormat(MapFileFormatKind.OCAD, 12));
            AddFileFormat(restrictToKind, MiscText.OCAD + " 2018", new MapFileFormat(MapFileFormatKind.OCAD, 2018));
            AddFileFormat(restrictToKind, MiscText.OpenOrienteeringMapper + " 0.7 (.omap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.OMap, 6));
            AddFileFormat(restrictToKind, MiscText.OpenOrienteeringMapper + " 0.7 (.xmap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.XMap, 6));
            AddFileFormat(restrictToKind, MiscText.OpenOrienteeringMapper + " 0.8 (.omap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.OMap, 7));
            AddFileFormat(restrictToKind, MiscText.OpenOrienteeringMapper + " 0.8 (.xmap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.XMap, 7));
            AddFileFormat(restrictToKind, MiscText.OpenOrienteeringMapper + " 0.9 (.omap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.OMap, 9));
            AddFileFormat(restrictToKind, MiscText.OpenOrienteeringMapper + " 0.9 (.xmap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.XMap, 9));

            int selectedIndex = fileFormatDescriptors.FindIndex(x => x.Equals(selectedFormat));
            SelectedFileFormatIndex = selectedIndex >= 0 ? selectedIndex : 0;
        }

        /// <summary>
        /// Adds one file format option if it matches the current restriction.
        /// </summary>
        /// <param name="restrictToKind">The format kind restriction, or None for no restriction.</param>
        /// <param name="label">The user-visible option label.</param>
        /// <param name="fileFormat">The format descriptor.</param>
        private void AddFileFormat(MapFileFormatKind restrictToKind, string label, MapFileFormat fileFormat)
        {
            if (restrictToKind == MapFileFormatKind.None || restrictToKind == fileFormat.kind) {
                FileFormatOptions.Add(label);
                fileFormatDescriptors.Add(fileFormat);
            }
        }
    }
}

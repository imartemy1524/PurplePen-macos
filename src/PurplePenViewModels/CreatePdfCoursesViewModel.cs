// CreatePdfCoursesViewModel.cs
//
// ViewModel for the course PDF export dialog.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PurplePen;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for selecting courses and output settings for course PDF export.
    /// </summary>
    public partial class CreatePdfCoursesViewModel : ViewModelBase
    {
        /// <summary>
        /// Selectable course rows.
        /// </summary>
        public ObservableCollection<PdfCourseItemViewModel> Courses { get; } = new ObservableCollection<PdfCourseItemViewModel>();

        /// <summary>
        /// Output location selector: 0 = same folder as map file, 1 = same folder as Purple Pen file, 2 = other folder.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOtherDirectoryVisible))]
        [NotifyPropertyChangedFor(nameof(IsOkEnabled))]
        private int outputLocationIndex = 1;

        /// <summary>
        /// The folder used when output location is "Other folder".
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOkEnabled))]
        private string outputDirectory = "";

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
        /// True when the "Other folder" path is visible.
        /// </summary>
        public bool IsOtherDirectoryVisible => OutputLocationIndex == 2;

        /// <summary>
        /// True when the current selections are valid.
        /// </summary>
        public bool IsOkEnabled {
            get {
                return Courses.Any(x => x.IsSelected) &&
                       (OutputLocationIndex != 2 || !string.IsNullOrWhiteSpace(OutputDirectory));
            }
        }

        /// <summary>
        /// The settings currently produced by the dialog.
        /// </summary>
        public CoursePdfSettings PdfSettings { get; private set; } = new CoursePdfSettings();

        /// <summary>
        /// Parameterless constructor for the designer.
        /// </summary>
        public CreatePdfCoursesViewModel()
        {
            CourseItemChanged += (_, _) => OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Initialize the dialog from the current event database.
        /// </summary>
        /// <param name="eventDB">The current event database.</param>
        /// <param name="anyMultipart">Whether any course has multiple parts.</param>
        /// <param name="settings">Optional preexisting settings.</param>
        public void Initialize(EventDB eventDB, bool anyMultipart, CoursePdfSettings? settings = null)
        {
            Courses.Clear();

            CoursePdfSettings useSettings = settings != null ? settings.Clone() : new CoursePdfSettings();

            PdfSettings = useSettings;
            FileCreationIndex = (int) useSettings.FileCreation;
            ColorModelIndex = (useSettings.ColorModel == ColorModel.RGB) ? 0 : 1;
            PrintBaseMapIndex = useSettings.DontPrintBaseMap ? 1 : 0;
            MultiPageIndex = useSettings.CropLargePrintArea ? 0 : 1;
            MergeParts = useSettings.PrintMapExchangesOnOneMap;

            if (useSettings.mapDirectory) {
                OutputLocationIndex = 0;
            }
            else if (useSettings.fileDirectory) {
                OutputLocationIndex = 1;
            }
            else {
                OutputLocationIndex = 2;
            }

            OutputDirectory = useSettings.outputDirectory ?? "";
            FilePrefix = useSettings.filePrefix ?? "";
            CanChangeCropping = true;

            Courses.Add(new PdfCourseItemViewModel(Id<Course>.None, MiscText.AllControls, useSettings.AllCourses));
            foreach (Id<Course> courseId in QueryEvent.SortedCourseIds(eventDB, true)) {
                Courses.Add(new PdfCourseItemViewModel(courseId, eventDB.GetCourse(courseId).name, useSettings.AllCourses || ContainsCourse(useSettings.CourseIds, courseId)));
            }

            HookCourseItemNotifications();
            NormalizeLocation();
            NormalizeOutputDirectory(eventDB);

            if (!anyMultipart) {
                MergeParts = false;
            }
        }

        /// <summary>
        /// Selects every course row.
        /// </summary>
        [RelayCommand]
        private void SelectAllCourses()
        {
            foreach (PdfCourseItemViewModel item in Courses) {
                item.IsSelected = true;
            }
        }

        /// <summary>
        /// Clears every course row.
        /// </summary>
        [RelayCommand]
        private void SelectNoCourses()
        {
            foreach (PdfCourseItemViewModel item in Courses) {
                item.IsSelected = false;
            }
        }

        /// <summary>
        /// Opens a folder picker for the "Other folder" location.
        /// </summary>
        [RelayCommand]
        private async Task SelectOtherDirectory()
        {
            FolderOpenViewModel folderOpenVm = new FolderOpenViewModel {
                Title = null,
                InitialDirectory = OutputDirectory
            };

            bool result = await Services.DialogService.ShowDialogAsync(folderOpenVm);
            if (result && folderOpenVm.SelectedFolder != null) {
                OutputDirectory = folderOpenVm.SelectedFolder;
                OnPropertyChanged(nameof(IsOkEnabled));
            }
        }

        /// <summary>
        /// Gets the settings represented by the current dialog values.
        /// </summary>
        public CoursePdfSettings BuildSettings()
        {
            CoursePdfSettings settings = PdfSettings != null ? PdfSettings.Clone() : new CoursePdfSettings();
            settings.CourseIds = Courses.Where(x => x.IsSelected).Select(x => x.CourseId).ToArray();
            settings.AllCourses = Courses.All(x => x.IsSelected);
            settings.FileCreation = (CoursePdfSettings.PdfFileCreation) FileCreationIndex;
            settings.ColorModel = (ColorModel) (ColorModelIndex + 1);
            settings.DontPrintBaseMap = (PrintBaseMapIndex == 1);
            settings.CropLargePrintArea = (MultiPageIndex == 0);
            settings.PrintMapExchangesOnOneMap = MergeParts;
            settings.mapDirectory = (OutputLocationIndex == 0);
            settings.fileDirectory = (OutputLocationIndex == 1);
            settings.outputDirectory = OutputDirectory;
            settings.filePrefix = FilePrefix;
            return settings;
        }

        /// <summary>
        /// Returns true when any course item changes selection.
        /// </summary>
        private event EventHandler? CourseItemChanged;

        /// <summary>
        /// Hooks change notifications from each course item.
        /// </summary>
        private void HookCourseItemNotifications()
        {
            foreach (PdfCourseItemViewModel item in Courses) {
                item.PropertyChanged -= CourseItemPropertyChanged;
                item.PropertyChanged += CourseItemPropertyChanged;
            }
        }

        /// <summary>
        /// Handles course item selection changes.
        /// </summary>
        private void CourseItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PdfCourseItemViewModel.IsSelected)) {
                CourseItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Keeps the output-location selection consistent.
        /// </summary>
        partial void OnOutputLocationIndexChanged(int value)
        {
            OnPropertyChanged(nameof(IsOtherDirectoryVisible));
            OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Keeps the OK button state in sync.
        /// </summary>
        partial void OnOutputDirectoryChanged(string value)
        {
            OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Keeps the OK button state in sync.
        /// </summary>
        partial void OnFilePrefixChanged(string value)
        {
            OnPropertyChanged(nameof(IsOkEnabled));
        }

        /// <summary>
        /// Normalizes the radio-button-like location selection.
        /// </summary>
        private void NormalizeLocation()
        {
            if (OutputLocationIndex < 0 || OutputLocationIndex > 2) {
                OutputLocationIndex = 1;
            }
        }

        /// <summary>
        /// Fills in a default output directory when possible.
        /// </summary>
        private void NormalizeOutputDirectory(EventDB eventDB)
        {
            if (string.IsNullOrWhiteSpace(OutputDirectory)) {
                string? mapFileName = eventDB.GetEvent().mapFileName;
                if (!string.IsNullOrWhiteSpace(mapFileName)) {
                    OutputDirectory = Path.GetDirectoryName(mapFileName) ?? "";
                }
            }
        }

        /// <summary>
        /// Returns true if the given course id is in an array.
        /// </summary>
        private static bool ContainsCourse(Id<Course>[]? courseIds, Id<Course> courseId)
        {
            if (courseIds == null)
                return false;
            foreach (Id<Course> id in courseIds) {
                if (id == courseId)
                    return true;
            }
            return false;
        }
    }
}

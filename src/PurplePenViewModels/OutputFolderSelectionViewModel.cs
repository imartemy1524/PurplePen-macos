// OutputFolderSelectionViewModel.cs
//
// Shared ViewModel for selecting an export output folder.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// Shared output folder selector for file export dialogs.
    /// </summary>
    public partial class OutputFolderSelectionViewModel : ViewModelBase
    {
        /// <summary>
        /// Raised when any output folder setting changes.
        /// </summary>
        public event EventHandler? SelectionChanged;

        /// <summary>
        /// Output location selector: 0 = same folder as Purple Pen file, 1 = same folder as map file, 2 = other folder.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOtherDirectoryVisible))]
        [NotifyPropertyChangedFor(nameof(IsValid))]
        [NotifyPropertyChangedFor(nameof(IsCoursesDirectory))]
        [NotifyPropertyChangedFor(nameof(IsMapDirectory))]
        [NotifyPropertyChangedFor(nameof(IsOtherDirectory))]
        private int outputLocationIndex;

        /// <summary>
        /// Folder used when output location is "Other folder".
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsValid))]
        private string outputDirectory = "";

        /// <summary>
        /// True when the "Other folder" path is visible.
        /// </summary>
        public bool IsOtherDirectoryVisible => OutputLocationIndex == 2;

        /// <summary>
        /// True when output should go beside the Purple Pen course file.
        /// </summary>
        public bool IsCoursesDirectory {
            get { return OutputLocationIndex == 0; }
            set {
                if (value) {
                    OutputLocationIndex = 0;
                }
            }
        }

        /// <summary>
        /// True when output should go beside the source map file.
        /// </summary>
        public bool IsMapDirectory {
            get { return OutputLocationIndex == 1; }
            set {
                if (value) {
                    OutputLocationIndex = 1;
                }
            }
        }

        /// <summary>
        /// True when output should go to a user-selected folder.
        /// </summary>
        public bool IsOtherDirectory {
            get { return OutputLocationIndex == 2; }
            set {
                if (value) {
                    OutputLocationIndex = 2;
                }
            }
        }

        /// <summary>
        /// True when the selected folder mode has enough data.
        /// </summary>
        public bool IsValid => OutputLocationIndex != 2 || !string.IsNullOrWhiteSpace(OutputDirectory);

        /// <summary>
        /// Initializes the selector from persisted settings.
        /// </summary>
        /// <param name="fileDirectory">Whether output goes beside the course file.</param>
        /// <param name="mapDirectory">Whether output goes beside the map file.</param>
        /// <param name="outputDirectory">The remembered custom output folder.</param>
        public void Initialize(bool fileDirectory, bool mapDirectory, string? outputDirectory)
        {
            if (fileDirectory) {
                OutputLocationIndex = 0;
            }
            else if (mapDirectory) {
                OutputLocationIndex = 1;
            }
            else {
                OutputLocationIndex = 2;
            }

            OutputDirectory = outputDirectory ?? "";
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Opens a folder picker for the "Other folder" output location.
        /// </summary>
        [RelayCommand]
        private async Task SelectOtherDirectory()
        {
            OutputLocationIndex = 2;
            FolderOpenViewModel folderOpenVm = new FolderOpenViewModel {
                Title = null,
                InitialDirectory = OutputDirectory
            };

            bool result = await Services.DialogService.ShowDialogAsync(folderOpenVm);
            if (result && folderOpenVm.SelectedFolder != null) {
                OutputDirectory = folderOpenVm.SelectedFolder;
            }
        }

        /// <summary>
        /// Handles output location changes.
        /// </summary>
        /// <param name="value">The new output location index.</param>
        partial void OnOutputLocationIndexChanged(int value)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles custom output folder changes.
        /// </summary>
        /// <param name="value">The new custom output folder.</param>
        partial void OnOutputDirectoryChanged(string value)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

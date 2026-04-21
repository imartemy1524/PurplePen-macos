// FolderOpenViewModel.cs
//
// ViewModel for selecting a folder via a platform folder-open dialog.
// Contains only platform-neutral dialog options and receives the selected path.

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for a folder-open dialog.
    /// Set the configuration properties before showing the dialog; after the
    /// dialog closes, read <see cref="SelectedFolder"/> for the result.
    /// </summary>
    public class FolderOpenViewModel : ViewModelBase
    {
        /// <summary>
        /// The title bar text of the dialog, or null to use the platform default.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The initial directory to browse from, or null to use the platform default.
        /// This is a legacy local-path hint.
        /// </summary>
        public string? InitialDirectory { get; set; }

        /// <summary>
        /// Optional bookmark id of the initial folder to browse from.
        /// Used on platforms where local paths are unavailable.
        /// </summary>
        public string? InitialDirectoryBookmark { get; set; }

        /// <summary>
        /// After the dialog closes, the full local path of the selected folder
        /// when available, or null if the platform does not expose paths.
        /// Prefer <see cref="SelectedFolderReference"/> for cross-platform access.
        /// </summary>
        public string? SelectedFolder { get; set; }

        /// <summary>
        /// After the dialog closes, the selected folder reference containing bookmark
        /// metadata, or null if cancelled.
        /// </summary>
        public SelectedStorageFolder? SelectedFolderReference { get; set; }
    }
}

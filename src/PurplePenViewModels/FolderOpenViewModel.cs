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
        /// </summary>
        public string? InitialDirectory { get; set; }

        /// <summary>
        /// After the dialog closes, the full path of the selected folder,
        /// or null if the user cancelled.
        /// </summary>
        public string? SelectedFolder { get; set; }
    }
}

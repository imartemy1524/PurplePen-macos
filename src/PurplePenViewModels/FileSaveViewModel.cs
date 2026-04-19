// FileSaveViewModel.cs
//
// ViewModel for saving a file via a platform file-save dialog.
// Contains only platform-neutral dialog options and receives the selected path.

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for a file-save dialog.
    /// Set the configuration properties before showing the dialog; after the
    /// dialog closes, read <see cref="SelectedFile"/> for the result.
    /// </summary>
    public class FileSaveViewModel : ViewModelBase
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
        /// The initially suggested file name, or null to use the platform default.
        /// </summary>
        public string? SuggestedFileName { get; set; }

        /// <summary>
        /// A Windows-style file filter string, e.g. "Purple Pen files|*.ppen|All files|*.*".
        /// Each pair of segments (display name, pattern) is separated by '|'.
        /// </summary>
        public string FileFilters { get; set; } = "";

        /// <summary>
        /// The default extension to append when the platform supports it.
        /// </summary>
        public string? DefaultExtension { get; set; }

        /// <summary>
        /// After the dialog closes, the full path of the selected file, or null if
        /// the user cancelled.
        /// </summary>
        public string? SelectedFile { get; set; }
    }
}

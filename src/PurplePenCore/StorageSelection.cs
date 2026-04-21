using System;
using System.IO;
using System.Threading.Tasks;

namespace PurplePen
{
    /// <summary>
    /// Represents a file selected via platform storage APIs.
    /// Carries bookmark information and optional stream openers so callers do not
    /// have to rely on a local file path.
    /// </summary>
    public sealed class SelectedStorageFile
    {
        private readonly Func<Task<Stream>>? openReadStream;
        private readonly Func<Task<Stream>>? openWriteStream;

        /// <summary>
        /// Creates a new selected file reference.
        /// </summary>
        /// <param name="localPath">Local file path when available; null on sandboxed platforms.</param>
        /// <param name="bookmarkId">Bookmark id when available; null if unsupported or denied.</param>
        /// <param name="openReadStream">Delegate used to open a read stream.</param>
        /// <param name="openWriteStream">Delegate used to open a write stream.</param>
        public SelectedStorageFile(string? localPath, string? bookmarkId, Func<Task<Stream>>? openReadStream, Func<Task<Stream>>? openWriteStream)
        {
            LocalPath = localPath;
            BookmarkId = bookmarkId;
            this.openReadStream = openReadStream;
            this.openWriteStream = openWriteStream;
        }

        /// <summary>
        /// Local file-system path when available.
        /// This can be null on browser/mobile sandboxed environments.
        /// </summary>
        public string? LocalPath { get; }

        /// <summary>
        /// Bookmark identifier for reopening this file on supported platforms.
        /// </summary>
        public string? BookmarkId { get; }

        /// <summary>
        /// True when a read stream can be opened for this selected file.
        /// </summary>
        public bool CanOpenReadStream => openReadStream != null;

        /// <summary>
        /// True when a write stream can be opened for this selected file.
        /// </summary>
        public bool CanOpenWriteStream => openWriteStream != null;

        /// <summary>
        /// Opens a read stream for the selected file.
        /// </summary>
        /// <returns>A readable stream for file content.</returns>
        public Task<Stream> OpenReadStreamAsync()
        {
            if (openReadStream == null)
                throw new InvalidOperationException("No read stream is available for this selected file.");

            return openReadStream();
        }

        /// <summary>
        /// Opens a write stream for the selected file.
        /// </summary>
        /// <returns>A writable stream for file content.</returns>
        public Task<Stream> OpenWriteStreamAsync()
        {
            if (openWriteStream == null)
                throw new InvalidOperationException("No write stream is available for this selected file.");

            return openWriteStream();
        }
    }

    /// <summary>
    /// Represents a folder selected via platform storage APIs.
    /// Carries bookmark information so callers do not have to rely on a local path.
    /// </summary>
    public sealed class SelectedStorageFolder
    {
        /// <summary>
        /// Creates a new selected folder reference.
        /// </summary>
        /// <param name="localPath">Local folder path when available; null on sandboxed platforms.</param>
        /// <param name="bookmarkId">Bookmark id when available; null if unsupported or denied.</param>
        public SelectedStorageFolder(string? localPath, string? bookmarkId)
        {
            LocalPath = localPath;
            BookmarkId = bookmarkId;
        }

        /// <summary>
        /// Local folder path when available.
        /// This can be null on browser/mobile sandboxed environments.
        /// </summary>
        public string? LocalPath { get; }

        /// <summary>
        /// Bookmark identifier for reopening this folder on supported platforms.
        /// </summary>
        public string? BookmarkId { get; }
    }
}

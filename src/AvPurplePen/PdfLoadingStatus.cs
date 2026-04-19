// PdfLoadingStatus.cs
//
// Minimal PDF conversion status implementation for the Avalonia port.

using PurplePen;
using System.Threading;

namespace AvPurplePen
{
    /// <summary>
    /// Waits for background PDF template conversion to finish.
    /// This keeps the current CoreMapUtil contract working until a proper
    /// Avalonia progress dialog is ported.
    /// </summary>
    public class PdfLoadingStatus : IPdfLoadingStatus
    {
        private readonly ManualResetEventSlim completionEvent = new ManualResetEventSlim(false);
        private bool success;

        /// <summary>
        /// Blocks until <see cref="LoadingComplete"/> is called by the converter.
        /// </summary>
        /// <param name="fileName">The PDF file being converted.</param>
        /// <returns>True if conversion succeeded; otherwise false.</returns>
        public bool ShowLoadingStatus(string fileName)
        {
            completionEvent.Wait();
            return success;
        }

        /// <summary>
        /// Records the conversion result and releases <see cref="ShowLoadingStatus"/>.
        /// </summary>
        /// <param name="success">True if conversion succeeded.</param>
        /// <param name="errorMessage">The conversion error text, if any.</param>
        public void LoadingComplete(bool success, string errorMessage)
        {
            this.success = success;
            completionEvent.Set();
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using PurplePen;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AvPurplePen
{
    /// <summary>
    /// Platform implementation of <see cref="IExternalLauncher"/> using Avalonia launch APIs.
    /// </summary>
    public sealed class ExternalLauncherService : IExternalLauncher
    {
        /// <summary>
        /// Opens a local file path in the default platform app.
        /// </summary>
        /// <param name="localPath">Local file path to launch.</param>
        /// <returns>True if launch is supported and accepted by OS; otherwise false.</returns>
        public async Task<bool> OpenLocalPathAsync(string localPath)
        {
            if (string.IsNullOrEmpty(localPath)) {
                return false;
            }

            ILauncher? launcher = GetLauncher();
            if (launcher == null) {
                return false;
            }

            try {
                return await launcher.LaunchFileInfoAsync(new FileInfo(localPath));
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Opens a URI in the default platform handler.
        /// </summary>
        /// <param name="uri">URI to launch.</param>
        /// <returns>True if launch is supported and accepted by OS; otherwise false.</returns>
        public async Task<bool> OpenUriAsync(string uri)
        {
            if (string.IsNullOrEmpty(uri)) {
                return false;
            }

            ILauncher? launcher = GetLauncher();
            if (launcher == null) {
                return false;
            }

            try {
                return await launcher.LaunchUriAsync(new Uri(uri));
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Opens HTML content as a data URI (browser-friendly).
        /// </summary>
        /// <param name="title">HTML page title.</param>
        /// <param name="htmlBody">HTML body fragment.</param>
        /// <returns>True if launch is supported and accepted by OS; otherwise false.</returns>
        public Task<bool> OpenHtmlContentAsync(string title, string htmlBody)
        {
            string fullHtml = WrapHtml(title, htmlBody);
            string encoded = Uri.EscapeDataString(fullHtml);
            string dataUri = "data:text/html;charset=utf-8," + encoded;
            return OpenUriAsync(dataUri);
        }

        /// <summary>
        /// Gets an Avalonia launcher from the active top-level host.
        /// </summary>
        /// <returns>Launcher when available; otherwise null.</returns>
        private static ILauncher? GetLauncher()
        {
            if (App.MainWindow != null) {
                return App.MainWindow.Launcher;
            }

            if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView &&
                singleView.MainView is Visual visual) {
                TopLevel? topLevel = TopLevel.GetTopLevel(visual);
                if (topLevel != null) {
                    return topLevel.Launcher;
                }
            }

            return null;
        }

        /// <summary>
        /// Wraps report HTML body in a complete document.
        /// </summary>
        /// <param name="title">Page title.</param>
        /// <param name="htmlBody">HTML body fragment.</param>
        /// <returns>Full HTML document.</returns>
        private static string WrapHtml(string title, string htmlBody)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<!DOCTYPE html>");
            builder.AppendLine("<html>");
            builder.AppendLine("<head>");
            builder.AppendLine("<meta charset='UTF-8'>");
            builder.Append("<title>").Append(title).AppendLine("</title>");
            builder.AppendLine("<style>");
            builder.AppendLine("body { font-family: Calibri, Arial, Helvetica, sans-serif; font-size: 12pt; margin: 20px; }");
            builder.AppendLine("table { border-collapse: collapse; }");
            builder.AppendLine("th, td { border: 1px solid #999; padding: 8px; }");
            builder.AppendLine("th { background-color: #f0f0f0; font-weight: bold; }");
            builder.AppendLine("h1, h2, h3 { color: #333; }");
            builder.AppendLine("</style>");
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            builder.Append("<h1>").Append(title).AppendLine("</h1>");
            builder.AppendLine(htmlBody);
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");
            return builder.ToString();
        }
    }
}

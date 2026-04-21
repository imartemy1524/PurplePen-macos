using System.Threading.Tasks;

namespace PurplePen
{
    /// <summary>
    /// Abstraction for opening files/URIs and browser-visible content.
    /// Implemented by the platform UI layer.
    /// </summary>
    public interface IExternalLauncher
    {
        /// <summary>
        /// Opens a local file path in the platform default application.
        /// </summary>
        /// <param name="localPath">Local file path to open.</param>
        /// <returns>True when launch was accepted by the platform; otherwise false.</returns>
        Task<bool> OpenLocalPathAsync(string localPath);

        /// <summary>
        /// Opens a URI in the platform default handler.
        /// </summary>
        /// <param name="uri">URI to open.</param>
        /// <returns>True when launch was accepted by the platform; otherwise false.</returns>
        Task<bool> OpenUriAsync(string uri);

        /// <summary>
        /// Opens HTML content in a browser-friendly way.
        /// </summary>
        /// <param name="title">Page title.</param>
        /// <param name="htmlBody">HTML body fragment to present.</param>
        /// <returns>True when launch was accepted by the platform; otherwise false.</returns>
        Task<bool> OpenHtmlContentAsync(string title, string htmlBody);
    }
}

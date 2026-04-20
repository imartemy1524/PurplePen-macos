// IUITheme.cs
//
// Service interface for getting and setting the application UI theme.

namespace PurplePen
{
    /// <summary>
    /// Provides access to the current application theme and allows changing it at runtime.
    /// </summary>
    public interface IUITheme
    {
        /// <summary>
        /// Gets or sets the current theme code.
        /// Supported values: "System", "Light", "Dark".
        /// </summary>
        string ThemeName { get; set; }
    }
}

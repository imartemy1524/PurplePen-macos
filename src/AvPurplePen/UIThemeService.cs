// UIThemeService.cs
//
// Avalonia implementation of IUITheme. Changes App.RequestedThemeVariant.

using Avalonia.Styling;
using PurplePen;
using System;

namespace AvPurplePen
{
    /// <summary>
    /// Manages the application theme for the Avalonia application.
    /// </summary>
    public class UIThemeService : IUITheme
    {
        /// <summary>
        /// Gets or sets the current theme code ("System", "Light", "Dark").
        /// </summary>
        public string ThemeName
        {
            get => App.ThemeVariantToSettingName(App.Current?.RequestedThemeVariant ?? ThemeVariant.Default);
            set
            {
                ThemeVariant requestedThemeVariant = App.SettingNameToThemeVariant(value);
                if (App.Current != null) {
                    App.Current.RequestedThemeVariant = requestedThemeVariant;
                }
            }
        }
    }
}

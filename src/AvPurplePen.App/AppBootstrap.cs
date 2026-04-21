using Avalonia;
using Map_SkiaStd;
using Microsoft.Extensions.DependencyInjection;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using PurplePen.ViewModels;
using System;
using System.Globalization;
using System.IO;

namespace AvPurplePen
{
    /// <summary>
    /// Shared startup/bootstrap logic used by all hosts (desktop and browser).
    /// </summary>
    public static class AppBootstrap
    {
        /// <summary>
        /// Initializes services and shared application state before creating Avalonia lifetimes.
        /// </summary>
        public static void InitializeBeforeStart()
        {
            RegisterServices();

            string userSettingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PurplePen",
                "PurplePenSettings.json");
            UserSettings.Initialize(userSettingsPath);

            InitUILanguage();
            FontDesc.InitializeFonts();
        }

        /// <summary>
        /// Creates the base Avalonia app builder used by all hosts.
        /// </summary>
        public static AppBuilder CreateAppBuilder()
        {
            return AppBuilder.Configure<App>()
                .LogToTrace();
        }

        /// <summary>
        /// Initializes all services required by PurplePenCore and the app shell.
        /// </summary>
        private static void RegisterServices()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<IGraphicsBitmapLoader, SkiaBitmapGraphicsLoader>();
            services.AddSingleton<IBitmapGraphicsTargetProvider, SkiaBitmapGraphicsTargetProvider>();
            services.AddSingleton<IFontLoader>(SkiaFontLoader.Instance);
            services.AddSingleton<ITextMetrics, Skia_TextMetrics>();
            services.AddSingleton<IFileLoaderProvider, SkiaFileLoaderProvider>();
            services.AddSingleton<IPdfWriter, PdfWriter>();
            services.AddSingleton<IApplicationIdleService, ApplicationIdleServiceAdapter>();
            services.AddTransient<IPdfLoadingStatus, PdfLoadingStatus>();

            // IDialogService depends on MainWindow, which is created later by App.
            services.AddSingleton<IDialogService>(sp => new DialogService(App.MainWindow!));
            services.AddSingleton<IUILanguage, UILanguageService>();
            services.AddSingleton<IUITheme, UIThemeService>();
            services.AddSingleton<IPostMessage, PostMessageService>();
            services.AddSingleton<IExternalLauncher, ExternalLauncherService>();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            Services.RegisterServiceProvider(serviceProvider);
        }

        /// <summary>
        /// Initializes UI language from persisted user settings.
        /// </summary>
        private static void InitUILanguage()
        {
            string uiLanguage = UserSettings.Current.UILanguage;

            if (!string.IsNullOrEmpty(uiLanguage)) {
                try {
                    System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(uiLanguage);
                }
                catch (Exception) {
                    // Ignore unsupported culture names.
                }
            }
        }
    }
}

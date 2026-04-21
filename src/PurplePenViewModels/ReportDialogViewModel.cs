// ReportDialogViewModel.cs
//
// ViewModel for displaying reports in a dialog.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the report display dialog.
    /// </summary>
    public partial class ReportDialogViewModel : ViewModelBase
    {
        private string htmlContent = "";

        [ObservableProperty]
        private string reportTitle = "";

        [ObservableProperty]
        private string htmlContent_Property = "";

        /// <summary>
        /// Initialize the dialog with report data.
        /// </summary>
        public void Initialize(string title, string htmlBody)
        {
            ReportTitle = title;
            htmlContent = htmlBody;
            HtmlContent_Property = htmlBody;
        }

        /// <summary>
        /// Print the report.
        /// </summary>
        [RelayCommand]
        private async Task Print()
        {
            await OpenHtmlInDefaultBrowserAsync();
        }

        /// <summary>
        /// Print preview the report.
        /// </summary>
        [RelayCommand]
        private async Task PrintPreview()
        {
            await OpenHtmlInDefaultBrowserAsync();
        }

        /// <summary>
        /// Opens the HTML content in the default browser for printing.
        /// </summary>
        private async Task OpenHtmlInDefaultBrowserAsync()
        {
            try {
                bool launched = await Services.ExternalLauncher.OpenHtmlContentAsync(ReportTitle, htmlContent);
                if (!launched) {
                    Debug.WriteLine("Failed to launch report preview.");
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Failed to open browser: {ex}");
            }
        }
    }
}

// ReportDialogViewModel.cs
//
// ViewModel for displaying reports in a dialog.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
        private void Print()
        {
            OpenHtmlInDefaultBrowser();
        }

        /// <summary>
        /// Print preview the report.
        /// </summary>
        [RelayCommand]
        private void PrintPreview()
        {
            OpenHtmlInDefaultBrowser();
        }

        /// <summary>
        /// Opens the HTML content in the default browser for printing.
        /// </summary>
        private void OpenHtmlInDefaultBrowser()
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), $"PurplePen_{Path.GetRandomFileName()}.html");

                // Wrap the report content with proper HTML structure
                string fullHtml = $@"<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<title>{ReportTitle}</title>
<style>
body {{
    font-family: Calibri, Arial, Helvetica, sans-serif;
    font-size: 12pt;
    margin: 20px;
}}
table {{
    border-collapse: collapse;
}}
th, td {{
    border: 1px solid #999;
    padding: 8px;
}}
th {{
    background-color: #f0f0f0;
    font-weight: bold;
}}
h1, h2, h3 {{
    color: #333;
}}
</style>
</head>
<body>
<h1>{ReportTitle}</h1>
{htmlContent}
</body>
</html>";

                File.WriteAllText(tempPath, fullHtml, Encoding.UTF8);

                // Open in default browser
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", new string[] { tempPath });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", new string[] { tempPath });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open browser: {ex}");
            }
        }
    }
}

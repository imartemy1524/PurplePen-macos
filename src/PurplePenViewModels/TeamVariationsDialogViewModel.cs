// TeamVariationsDialogViewModel.cs
//
// ViewModel for the relay team variations report dialog.
// Mirrors the original WinForms TeamVariationsForm behavior closely enough
// for the macOS/Avalonia port: calculate, fixed legs, export, and browser
// preview of the generated report.
//
// Migrated from WinForms PurplePen/TeamVariationsForm.cs.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PurplePen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// One warning line in the relay variations report.
    /// </summary>
    public partial class RelayVariationWarningViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string text = "";
    }

    /// <summary>
    /// One row in the relay variations report table.
    /// </summary>
    public partial class RelayVariationRowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int teamNumber;

        public ObservableCollection<string> LegCodes { get; } = new ObservableCollection<string>();
    }

    /// <summary>
    /// ViewModel for the relay team variations dialog.
    /// </summary>
    public partial class TeamVariationsDialogViewModel : ViewModelBase
    {
        private const string RelayVariationNoTeamsText = "No relay teams have been defined yet. Selected the desired number of teams and legs and press the \"Assign Variations\" button.";
        private const string RelayVariationBranchWarningFormat = "Warning: due to the number of legs specified, the fork at control {0} will not be used evenly. {1} leg(s) will use branch(es) {2}, while {3} leg(s) will use branch(es) {4}. ";
        private const string RelayVariationLegHeaderFormat = "Leg {0}";
        private const string RelayVariationTeamNumberFormat = "Team {0}";
        private const string RelayVariationTitleFormat = "Relay Assignments for {0}";

        private Controller? controller;
        private string defaultExportFileName = "";

        /// <summary>
        /// The first team number.
        /// </summary>
        [ObservableProperty]
        private int firstTeamNumber = 1;

        /// <summary>
        /// The number of teams.
        /// </summary>
        [ObservableProperty]
        private int numberOfTeams;

        /// <summary>
        /// The number of legs.
        /// </summary>
        [ObservableProperty]
        private int numberOfLegs = 1;

        /// <summary>
        /// Whether to hide the variation codes on the map.
        /// </summary>
        [ObservableProperty]
        private bool hideVariationsOnMap;

        /// <summary>
        /// The current fixed branch assignments.
        /// </summary>
        [ObservableProperty]
        private FixedBranchAssignments fixedBranchAssignments = new FixedBranchAssignments();

        /// <summary>
        /// The title of the current report.
        /// </summary>
        [ObservableProperty]
        private string reportTitle = "";

        /// <summary>
        /// A status line shown when no report can be generated.
        /// </summary>
        [ObservableProperty]
        private string reportStatusText = "";

        /// <summary>
        /// HTML preview for print/preview actions.
        /// </summary>
        [ObservableProperty]
        private string reportHtml = "";

        /// <summary>
        /// Report warnings.
        /// </summary>
        public ObservableCollection<RelayVariationWarningViewModel> BranchWarnings { get; } = new ObservableCollection<RelayVariationWarningViewModel>();

        /// <summary>
        /// Report rows.
        /// </summary>
        public ObservableCollection<RelayVariationRowViewModel> Rows { get; } = new ObservableCollection<RelayVariationRowViewModel>();

        /// <summary>
        /// Header labels for the legs.
        /// </summary>
        public ObservableCollection<string> LegHeaders { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Initializes the dialog from the current controller state.
        /// </summary>
        public void Initialize(Controller controller, RelaySettings relaySettings, bool hideVariationsOnMap, string defaultExportFileName)
        {
            this.controller = controller;
            this.defaultExportFileName = defaultExportFileName ?? "";

            FirstTeamNumber = relaySettings.firstTeamNumber;
            NumberOfTeams = relaySettings.relayTeams;
            NumberOfLegs = relaySettings.relayLegs;
            HideVariationsOnMap = hideVariationsOnMap;
            FixedBranchAssignments = relaySettings.relayBranchAssignments.Clone();

            RefreshReport();
        }

        /// <summary>
        /// Returns the current relay settings represented by the dialog.
        /// </summary>
        public RelaySettings RelaySettings
        {
            get {
                return new RelaySettings(FirstTeamNumber, NumberOfTeams, NumberOfLegs, FixedBranchAssignments);
            }
        }

        /// <summary>
        /// Recalculates the report body from the current settings.
        /// </summary>
        [RelayCommand]
        private void Calculate()
        {
            RefreshReport();
        }

        /// <summary>
        /// Opens the fixed-leg assignments dialog.
        /// </summary>
        [RelayCommand]
        private async Task AssignLegsAsync()
        {
            if (controller == null)
                return;

            LegAssignmentsDialogViewModel dialogVm = new LegAssignmentsDialogViewModel();
            dialogVm.Initialize(controller, controller.GetLegAssignmentCodes(), FixedBranchAssignments, NumberOfLegs);

            bool result = await Services.DialogService.ShowDialogAsync(dialogVm);
            if (result) {
                FixedBranchAssignments = dialogVm.FixedBranchAssignments;
                RefreshReport();
            }
        }

        /// <summary>
        /// Exports the current report to XML or CSV.
        /// </summary>
        [RelayCommand]
        private async Task ExportAsync()
        {
            if (controller == null)
                return;

            FileSaveViewModel fileSaveVm = new FileSaveViewModel {
                Title = "Relay team variations",
                InitialDirectory = Path.GetDirectoryName(defaultExportFileName),
                SuggestedFileName = Path.GetFileName(defaultExportFileName),
                FileFilters = "XML files|*.xml|CSV files|*.csv",
                DefaultExtension = "xml"
            };

            bool result = await Services.DialogService.ShowDialogAsync(fileSaveVm);
            if (!result || string.IsNullOrEmpty(fileSaveVm.SelectedFile))
                return;

            VariationExportFileType exportFileType;
            if (!string.IsNullOrEmpty(fileSaveVm.SelectedFile) && fileSaveVm.SelectedFile.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) {
                exportFileType = VariationExportFileType.Csv;
            }
            else {
                exportFileType = VariationExportFileType.Xml;
            }

            controller.ExportRelayVariationsReport(RelaySettings, exportFileType, fileSaveVm.SelectedFile!);
        }

        /// <summary>
        /// Opens the generated HTML report in the user's default browser.
        /// </summary>
        [RelayCommand]
        private async Task PrintAsync()
        {
            await OpenHtmlPreviewAsync();
        }

        /// <summary>
        /// Opens the generated HTML report in the user's default browser.
        /// </summary>
        [RelayCommand]
        private async Task PrintPreviewAsync()
        {
            await OpenHtmlPreviewAsync();
        }

        private void RefreshReport()
        {
            BranchWarnings.Clear();
            Rows.Clear();
            LegHeaders.Clear();

            if (controller == null)
                return;

            if (NumberOfTeams <= 0) {
                ReportTitle = "Relay Assignments";
                ReportStatusText = RelayVariationNoTeamsText;
                ReportHtml = BuildHtmlReport(ReportTitle, ReportStatusText, "", Array.Empty<string>(), Array.Empty<RelayVariationRowViewModel>());
                return;
            }

            VariationReportData variationReportData = controller.GetVariationReportData(RelaySettings);
            RelayVariations relayVariations = variationReportData.RelayVariations;

            ReportTitle = string.Format(CultureInfo.CurrentCulture, RelayVariationTitleFormat, variationReportData.CourseName);
            ReportStatusText = "";

            foreach (RelayVariations.BranchWarning branchWarning in relayVariations.GetBranchWarnings()) {
                string codesMore = string.Join(", ", branchWarning.codeMore);
                string codesLess = string.Join(", ", branchWarning.codeLess);
                BranchWarnings.Add(new RelayVariationWarningViewModel {
                    Text = string.Format(CultureInfo.CurrentCulture, RelayVariationBranchWarningFormat, branchWarning.ControlCode, branchWarning.numMore, codesMore, branchWarning.numLess, codesLess)
                });
            }

            for (int legNumber = 1; legNumber <= relayVariations.NumberOfLegs; ++legNumber) {
                LegHeaders.Add(string.Format(CultureInfo.CurrentCulture, RelayVariationLegHeaderFormat, legNumber));
            }

            for (int teamNumber = relayVariations.FirstTeamNumber; teamNumber <= relayVariations.LastTeamNumber; ++teamNumber) {
                RelayVariationRowViewModel row = new RelayVariationRowViewModel {
                    TeamNumber = teamNumber
                };

                for (int legNumber = 1; legNumber <= relayVariations.NumberOfLegs; ++legNumber) {
                    row.LegCodes.Add(relayVariations.GetVariation(teamNumber, legNumber).CodeString);
                }

                Rows.Add(row);
            }

            ReportHtml = BuildHtmlReport(ReportTitle, ReportStatusText, "", GetWarningTexts(), GetRowsForHtml());
        }

        private string[] GetWarningTexts()
        {
            List<string> warnings = new List<string>();
            foreach (RelayVariationWarningViewModel warning in BranchWarnings) {
                warnings.Add(warning.Text);
            }

            return warnings.ToArray();
        }

        private RelayVariationRowViewModel[] GetRowsForHtml()
        {
            return Rows.ToArray();
        }

        private static string BuildHtmlReport(string title, string statusText, string bodyText, string[] warnings, RelayVariationRowViewModel[] rows)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head><meta charset=\"utf-8\"/>");
            html.AppendLine("<title>" + WebUtility.HtmlEncode(title) + "</title>");
            html.AppendLine("<style>");
            html.AppendLine("body{font-family:Calibri,Arial,sans-serif;font-size:12pt;margin:24px;}");
            html.AppendLine("h1{font-size:19pt;margin:0 0 12px 0;}");
            html.AppendLine("p{margin:0 0 8px 0;}");
            html.AppendLine("table{border-collapse:collapse;margin-top:12px;}");
            html.AppendLine("th,td{padding:4px 10px 4px 0;border-bottom:1px solid #ccc;}");
            html.AppendLine("th{font-weight:bold;}");
            html.AppendLine("td.team{padding-right:18px;white-space:nowrap;}");
            html.AppendLine("td.leg{text-align:right;min-width:48px;}");
            html.AppendLine(".warning{margin:0 0 8px 0;color:#8a4b00;}");
            html.AppendLine("</style></head><body>");
            html.AppendLine("<h1>" + WebUtility.HtmlEncode(title) + "</h1>");

            if (!string.IsNullOrEmpty(statusText)) {
                html.AppendLine("<p>" + WebUtility.HtmlEncode(statusText) + "</p>");
            }

            foreach (string warning in warnings) {
                html.AppendLine("<p class=\"warning\">" + WebUtility.HtmlEncode(warning) + "</p>");
            }

            if (rows.Length > 0) {
                html.AppendLine("<table><thead><tr><th></th>");
                for (int i = 0; i < rows[0].LegCodes.Count; ++i) {
                    html.AppendLine("<th>" + WebUtility.HtmlEncode(string.Format(CultureInfo.CurrentCulture, RelayVariationLegHeaderFormat, i + 1)) + "</th>");
                }
                html.AppendLine("</tr></thead><tbody>");
                foreach (RelayVariationRowViewModel row in rows) {
                    html.AppendLine("<tr><td class=\"team\">" + WebUtility.HtmlEncode(string.Format(CultureInfo.CurrentCulture, RelayVariationTeamNumberFormat, row.TeamNumber)) + "</td>");
                    foreach (string code in row.LegCodes) {
                        html.AppendLine("<td class=\"leg\">" + WebUtility.HtmlEncode(code) + "</td>");
                    }
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</tbody></table>");
            }

            if (!string.IsNullOrEmpty(bodyText)) {
                html.AppendLine("<p>" + WebUtility.HtmlEncode(bodyText) + "</p>");
            }

            html.AppendLine("</body></html>");
            return html.ToString();
        }

        private async Task OpenHtmlPreviewAsync()
        {
            if (string.IsNullOrEmpty(ReportHtml)) {
                RefreshReport();
            }

            string tempFile = Path.Combine(Path.GetTempPath(), "PurplePen-RelayVariations.html");
            await File.WriteAllTextAsync(tempFile, ReportHtml, Encoding.UTF8);

            Process.Start(new ProcessStartInfo {
                FileName = tempFile,
                UseShellExecute = true
            });
        }
    }
}

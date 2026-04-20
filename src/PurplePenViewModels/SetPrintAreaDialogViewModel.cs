// SetPrintAreaDialogViewModel.cs
//
// ViewModel for editing the course print area.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Set Print Area dialog.
    /// </summary>
    public partial class SetPrintAreaDialogViewModel : ViewModelBase
    {
        private Controller? controller;
        private PrintAreaKind printAreaKind;
        private PrintArea printArea = PrintArea.DefaultPrintArea;
        private bool updateInProgress;

        /// <summary>
        /// Available standard paper size names.
        /// </summary>
        public ObservableCollection<string> PaperSizeOptions { get; } = new ObservableCollection<string>();

        /// <summary>
        /// True when Purple Pen computes the print area automatically.
        /// </summary>
        [ObservableProperty]
        private bool automatic = true;

        /// <summary>
        /// True when the rectangle must keep the selected paper aspect and size.
        /// </summary>
        [ObservableProperty]
        private bool restrictToPageSize = true;

        /// <summary>
        /// Index of the selected standard paper size.
        /// </summary>
        [ObservableProperty]
        private int selectedPaperSizeIndex;

        /// <summary>
        /// True when the paper is used in landscape orientation.
        /// </summary>
        [ObservableProperty]
        private bool landscape;

        /// <summary>
        /// Margin in 1/100 inch units.
        /// </summary>
        [ObservableProperty]
        private int marginHundredths;

        /// <summary>
        /// Parameterless constructor for the designer.
        /// </summary>
        public SetPrintAreaDialogViewModel()
        {
            InitializePaperSizes();
        }

        /// <summary>
        /// Initializes the dialog from the current controller state.
        /// </summary>
        /// <param name="controller">The active controller.</param>
        /// <param name="printAreaKind">The print area scope being edited.</param>
        public void Initialize(Controller controller, PrintAreaKind printAreaKind)
        {
            this.controller = controller;
            this.printAreaKind = printAreaKind;
            printArea = (PrintArea)controller.GetCurrentPrintArea(printAreaKind).Clone();
            EnsureKnownPageSize();
            UpdatePropertiesFromPrintArea();
        }

        /// <summary>
        /// Sends the current settings to the active rectangle selection mode.
        /// </summary>
        public void SendCurrentSettingsToController()
        {
            if (controller == null) { return; }

            UpdatePrintAreaFromProperties();
            controller.SetPrintAreaUpdate(printAreaKind, printArea);
        }

        /// <summary>
        /// Applies the edited print area and exits rectangle selection mode.
        /// </summary>
        public void Apply()
        {
            if (controller == null) { return; }

            UpdatePrintAreaFromProperties();
            printArea.printAreaRectangle = controller.SetPrintAreaCurrentRectangle();
            controller.EndSetPrintArea(printAreaKind, printArea);
        }

        /// <summary>
        /// Cancels rectangle selection mode without saving changes.
        /// </summary>
        public void Cancel()
        {
            if (controller == null) { return; }

            controller.CancelMode();
        }

        /// <summary>
        /// Checks whether a dragged automatic rectangle has become manual.
        /// </summary>
        public void DetectManualRectangleChange()
        {
            if (controller == null || !Automatic) { return; }

            PrintArea defaultPrintArea = (PrintArea)printArea.Clone();
            defaultPrintArea.autoPrintArea = true;
            RectangleF defaultRectangle = controller.GetPrintAreaRectangle(printAreaKind, defaultPrintArea);
            if (controller.SetPrintAreaCurrentRectangle() != defaultRectangle) {
                updateInProgress = true;
                Automatic = false;
                printArea.autoPrintArea = false;
                updateInProgress = false;
            }
        }

        partial void OnAutomaticChanged(bool value)
        {
            if (!updateInProgress) {
                bool wasAutomatic = printArea.autoPrintArea;
                UpdatePrintAreaFromProperties();
                if (wasAutomatic && !printArea.autoPrintArea && controller != null) {
                    printArea.autoPrintArea = true;
                    printArea.printAreaRectangle = controller.GetPrintAreaRectangle(printAreaKind, printArea);
                    printArea.autoPrintArea = false;
                }

                SendCurrentSettingsToController();
            }
        }

        partial void OnRestrictToPageSizeChanged(bool value)
        {
            SendChangedSettings();
        }

        partial void OnSelectedPaperSizeIndexChanged(int value)
        {
            SendChangedSettings();
        }

        partial void OnLandscapeChanged(bool value)
        {
            SendChangedSettings();
        }

        partial void OnMarginHundredthsChanged(int value)
        {
            SendChangedSettings();
        }

        /// <summary>
        /// Sends changed settings when property updates are user initiated.
        /// </summary>
        private void SendChangedSettings()
        {
            if (!updateInProgress) {
                SendCurrentSettingsToController();
            }
        }

        /// <summary>
        /// Populates the standard paper size choices.
        /// </summary>
        private void InitializePaperSizes()
        {
            if (PaperSizeOptions.Count > 0) { return; }

            foreach (PrintingPaperSize paperSize in PrintingStandards.StandardPaperSizes) {
                PaperSizeOptions.Add(Util.GetPaperSizeText(paperSize));
            }
        }

        /// <summary>
        /// Ensures old events with unknown paper size get a usable default.
        /// </summary>
        private void EnsureKnownPageSize()
        {
            if (printArea.pageWidth > 0 && printArea.pageHeight > 0) {
                return;
            }

            bool metric = Util.IsCurrentCultureMetric();
            PrintingPaperSize paperSize = PrintingStandards.StandardPaperSizes[
                metric ? PrintingStandards.DefaultMetricPaperSizeindex : PrintingStandards.DefaultEnglighPaperSizeIndex];
            printArea.pageWidth = (int)Math.Round(paperSize.SizeInHundreths.Width);
            printArea.pageHeight = (int)Math.Round(paperSize.SizeInHundreths.Height);
            printArea.pageMargins = metric ? PrintingStandards.DefaultMetricMarginInHundreths : PrintingStandards.DefaultEnglishMarginInHundreths;
            printArea.pageLandscape = false;
        }

        /// <summary>
        /// Copies the current print area into bindable properties.
        /// </summary>
        private void UpdatePropertiesFromPrintArea()
        {
            updateInProgress = true;

            Automatic = printArea.autoPrintArea;
            RestrictToPageSize = printArea.restrictToPageSize;
            Landscape = printArea.pageLandscape;
            MarginHundredths = Math.Max(0, printArea.pageMargins);
            SelectedPaperSizeIndex = FindPaperSizeIndex(printArea.pageWidth, printArea.pageHeight);

            updateInProgress = false;
        }

        /// <summary>
        /// Copies bindable properties into the print area model.
        /// </summary>
        private void UpdatePrintAreaFromProperties()
        {
            printArea.autoPrintArea = Automatic;
            printArea.restrictToPageSize = RestrictToPageSize;
            PrintingPaperSize paperSize = PrintingStandards.StandardPaperSizes[Math.Clamp(SelectedPaperSizeIndex, 0, PrintingStandards.StandardPaperSizes.Length - 1)];
            printArea.pageWidth = (int)Math.Round(paperSize.SizeInHundreths.Width);
            printArea.pageHeight = (int)Math.Round(paperSize.SizeInHundreths.Height);
            printArea.pageLandscape = Landscape;
            printArea.pageMargins = Math.Max(0, MarginHundredths);
            if (controller != null) {
                printArea.printAreaRectangle = controller.SetPrintAreaCurrentRectangle();
            }
        }

        /// <summary>
        /// Finds the standard paper size matching the stored page dimensions.
        /// </summary>
        /// <param name="width">Page width in 1/100 inch units.</param>
        /// <param name="height">Page height in 1/100 inch units.</param>
        /// <returns>The paper size index, or the culture default when no exact match exists.</returns>
        private static int FindPaperSizeIndex(int width, int height)
        {
            for (int index = 0; index < PrintingStandards.StandardPaperSizes.Length; ++index) {
                PrintingPaperSize paperSize = PrintingStandards.StandardPaperSizes[index];
                if ((int)Math.Round(paperSize.SizeInHundreths.Width) == width &&
                    (int)Math.Round(paperSize.SizeInHundreths.Height) == height) {
                    return index;
                }
            }

            return Util.IsCurrentCultureMetric() ? PrintingStandards.DefaultMetricPaperSizeindex : PrintingStandards.DefaultEnglighPaperSizeIndex;
        }
    }
}

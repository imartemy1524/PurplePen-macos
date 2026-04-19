// NewEventWizardViewModel.cs
//
// ViewModel for the New Event wizard. It mirrors the legacy WinForms wizard
// flow and produces Controller.CreateEventInfo for Controller.NewEvent().

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the New Event wizard.
    /// It validates the selected map file, gathers wizard settings, and
    /// creates the event creation payload consumed by <see cref="Controller.NewEvent"/>.
    /// </summary>
    public partial class NewEventWizardViewModel : ViewModelBase
    {
        private const string MapFileFilter = "All map files|*.ocd;*.omap;*.xmap;*.pdf;*.jpeg;*.jpg;*.tiff;*.tif;*.bmp;*.png;*.gif|OCAD files (*.ocd)|*.ocd|Open Orienteering Mapper Files|*.omap;*.xmap|PDF files (*.pdf)|*.pdf|Image files|*.jpeg;*.jpg;*.tiff;*.tif;*.bmp;*.png;*.gif";

        private enum WizardPage
        {
            Title = 0,
            MapFile = 1,
            BitmapScale = 2,
            PrintScale = 3,
            PaperSize = 4,
            Directory = 5,
            Standards = 6,
            Numbering = 7,
            Final = 8
        }

        private float mapScale;
        private float defaultPrintScale;
        private float bitmapDpi;
        private Size bitmapSize;
        private RectangleF mapBounds;
        private int? lowerPurpleMapLayer;
        private bool paperSizeInitialized;

        /// <summary>
        /// The zero-based index of the active wizard page.
        /// </summary>
        [ObservableProperty]
        private int currentPageIndex;

        /// <summary>
        /// The event title entered on the first page.
        /// </summary>
        [ObservableProperty]
        private string eventTitle = "";

        /// <summary>
        /// The selected map file path.
        /// </summary>
        [ObservableProperty]
        private string mapFileName = "";

        /// <summary>
        /// The selected map type returned by map validation.
        /// </summary>
        [ObservableProperty]
        private MapType selectedMapType = MapType.None;

        /// <summary>
        /// Whether the selected map file loaded successfully.
        /// </summary>
        [ObservableProperty]
        private bool isMapFileInfoVisible;

        /// <summary>
        /// Whether the selected map file failed validation.
        /// </summary>
        [ObservableProperty]
        private bool isMapFileErrorVisible;

        /// <summary>
        /// The map validation error to show on the map file page.
        /// </summary>
        [ObservableProperty]
        private string mapFileErrorMessage = "";

        /// <summary>
        /// The bitmap/PDF map scale text.
        /// </summary>
        [ObservableProperty]
        private string bitmapMapScaleText = "15000";

        /// <summary>
        /// The bitmap DPI text. Hidden for PDF maps.
        /// </summary>
        [ObservableProperty]
        private string dpiText = "";

        /// <summary>
        /// The editable print scale text.
        /// </summary>
        [ObservableProperty]
        private string printScaleText = "";

        /// <summary>
        /// Whether the event file should be saved next to the map file.
        /// </summary>
        [ObservableProperty]
        private bool useMapDirectory = true;

        /// <summary>
        /// The explicitly chosen event directory.
        /// </summary>
        [ObservableProperty]
        private string eventDirectory = "";

        /// <summary>
        /// Width of the default print page in hundredths of an inch.
        /// </summary>
        [ObservableProperty]
        private int pageWidth = 850;

        /// <summary>
        /// Height of the default print page in hundredths of an inch.
        /// </summary>
        [ObservableProperty]
        private int pageHeight = 1100;

        /// <summary>
        /// Margin of the default print page in hundredths of an inch.
        /// </summary>
        [ObservableProperty]
        private int pageMargins;

        /// <summary>
        /// Whether the default print page is landscape.
        /// </summary>
        [ObservableProperty]
        private bool pageLandscape;

        /// <summary>
        /// Whether ISOM 2017 is selected.
        /// </summary>
        [ObservableProperty]
        private bool mapStandard2017 = true;

        /// <summary>
        /// Whether ISSprOM 2019 is selected.
        /// </summary>
        [ObservableProperty]
        private bool mapStandardSpr2019;

        /// <summary>
        /// Whether ISOM 2000 / ISSOM 2007 is selected.
        /// </summary>
        [ObservableProperty]
        private bool mapStandard2000;

        /// <summary>
        /// Whether description standard 2018/2024 is selected.
        /// </summary>
        [ObservableProperty]
        private bool descriptionStandard2018 = true;

        /// <summary>
        /// Whether description standard 2004 is selected.
        /// </summary>
        [ObservableProperty]
        private bool descriptionStandard2004;

        /// <summary>
        /// First control code to use for automatic numbering.
        /// </summary>
        [ObservableProperty]
        private int startingCode = 31;

        /// <summary>
        /// Whether automatically assigned control codes may be invertible.
        /// </summary>
        [ObservableProperty]
        private bool disallowInvertibleCodes;

        /// <summary>
        /// Whether the final page creation check failed.
        /// </summary>
        [ObservableProperty]
        private bool isFinalErrorVisible;

        /// <summary>
        /// The final page creation error.
        /// </summary>
        [ObservableProperty]
        private string finalErrorMessage = "";

        /// <summary>
        /// Available print scale values for the print scale page.
        /// </summary>
        public ObservableCollection<string> AvailablePrintScales { get; } = new ObservableCollection<string>();

        /// <summary>
        /// The event creation info populated when the wizard finishes.
        /// </summary>
        public Controller.CreateEventInfo CreateEventInfo { get; private set; }

        /// <summary>
        /// Parameterless constructor for the Avalonia designer and dialog service.
        /// Initializes default standards from persisted user settings.
        /// </summary>
        public NewEventWizardViewModel()
        {
            string mapStandard = UserSettings.Current.NewEventMapStandard;
            MapStandard2017 = mapStandard == "2017";
            MapStandardSpr2019 = mapStandard == "Spr2019";
            MapStandard2000 = !MapStandard2017 && !MapStandardSpr2019;

            string descriptionStandard = UserSettings.Current.NewEventDescriptionStandard;
            DescriptionStandard2018 = descriptionStandard == "2018";
            DescriptionStandard2004 = !DescriptionStandard2018;

            PopulatePrintScales(15000);
            PrintScaleText = "15000";
        }

        /// <summary>
        /// True when the first page is active.
        /// </summary>
        public bool IsTitlePage => CurrentPageIndex == (int)WizardPage.Title;

        /// <summary>
        /// True when the map file page is active.
        /// </summary>
        public bool IsMapFilePage => CurrentPageIndex == (int)WizardPage.MapFile;

        /// <summary>
        /// True when the bitmap/PDF scale page is active.
        /// </summary>
        public bool IsBitmapScalePage => CurrentPageIndex == (int)WizardPage.BitmapScale;

        /// <summary>
        /// True when the print scale page is active.
        /// </summary>
        public bool IsPrintScalePage => CurrentPageIndex == (int)WizardPage.PrintScale;

        /// <summary>
        /// True when the paper size page is active.
        /// </summary>
        public bool IsPaperSizePage => CurrentPageIndex == (int)WizardPage.PaperSize;

        /// <summary>
        /// True when the event directory page is active.
        /// </summary>
        public bool IsDirectoryPage => CurrentPageIndex == (int)WizardPage.Directory;

        /// <summary>
        /// True when the standards page is active.
        /// </summary>
        public bool IsStandardsPage => CurrentPageIndex == (int)WizardPage.Standards;

        /// <summary>
        /// True when the numbering page is active.
        /// </summary>
        public bool IsNumberingPage => CurrentPageIndex == (int)WizardPage.Numbering;

        /// <summary>
        /// True when the final page is active.
        /// </summary>
        public bool IsFinalPage => CurrentPageIndex == (int)WizardPage.Final;

        /// <summary>
        /// True when the wizard is not on the final page.
        /// </summary>
        public bool IsNotFinalPage => !IsFinalPage;

        /// <summary>
        /// True when the Back button is enabled.
        /// </summary>
        public bool IsBackEnabled => CurrentPageIndex > 0;

        /// <summary>
        /// True when the Next/Finish button is enabled.
        /// </summary>
        public bool IsNextEnabled => CanProceedFromCurrentPage();

        /// <summary>
        /// True when the selected map is a bitmap.
        /// </summary>
        public bool IsBitmapMap => SelectedMapType == MapType.Bitmap;

        /// <summary>
        /// True when the selected map is a PDF.
        /// </summary>
        public bool IsPdfMap => SelectedMapType == MapType.PDF;

        /// <summary>
        /// True when a map file has been selected.
        /// </summary>
        public bool HasMapFile => MapFileName.Length > 0;

        /// <summary>
        /// True when the user is choosing a directory other than the map directory.
        /// </summary>
        public bool UseOtherDirectory
        {
            get { return !UseMapDirectory; }
            set { UseMapDirectory = !value; }
        }

        /// <summary>
        /// The native map scale displayed on the print scale page.
        /// </summary>
        public string MapScaleDisplay => mapScale > 0 ? mapScale.ToString(CultureInfo.CurrentCulture) : "";

        /// <summary>
        /// The computed event file path displayed on the final page.
        /// </summary>
        public string EventFileFullPath => GetEventFullPath();

        /// <summary>
        /// Opens a platform file picker and validates the selected map file.
        /// </summary>
        [RelayCommand]
        private async Task ChooseMapFile()
        {
            FileOpenSingleViewModel fileOpenVm = new FileOpenSingleViewModel {
                FileFilters = MapFileFilter,
                InitialFileFilterIndex = 1
            };

            bool result = await Services.DialogService.ShowDialogAsync(fileOpenVm);
            if (result && fileOpenVm.SelectedFile != null) {
                SetMapFile(fileOpenVm.SelectedFile);
            }
        }

        /// <summary>
        /// Opens a platform folder picker and stores the selected event directory.
        /// </summary>
        [RelayCommand]
        private async Task ChooseEventDirectory()
        {
            FolderOpenViewModel folderOpenVm = new FolderOpenViewModel {
                Title = "Select the folder for your event file",
                InitialDirectory = GetInitialDirectoryForFolderPicker()
            };

            bool result = await Services.DialogService.ShowDialogAsync(folderOpenVm);
            if (result && folderOpenVm.SelectedFolder != null) {
                EventDirectory = folderOpenVm.SelectedFolder;
                UseMapDirectory = false;
            }
        }

        /// <summary>
        /// Moves back to the previous visible wizard page.
        /// </summary>
        public void Back()
        {
            if (CurrentPageIndex <= 0) {
                return;
            }

            int nextPage = CurrentPageIndex - 1;
            if ((WizardPage)nextPage == WizardPage.BitmapScale && !NeedsBitmapScalePage()) {
                nextPage -= 1;
            }

            GoToPage(nextPage);
        }

        /// <summary>
        /// Advances to the next page, or creates <see cref="CreateEventInfo"/>
        /// and validates the target event file when the final page is active.
        /// </summary>
        /// <returns>True when the wizard should close with an accepted result.</returns>
        public Task<bool> NextOrFinishAsync()
        {
            if (!CanProceedFromCurrentPage()) {
                return Task.FromResult(false);
            }

            if (IsFinalPage) {
                SetCreateInfo();
                if (TryCreateEvent(out string errorMessageText)) {
                    return Task.FromResult(true);
                }

                FinalErrorMessage = errorMessageText;
                IsFinalErrorVisible = true;
                return Task.FromResult(false);
            }

            int nextPage = CurrentPageIndex + 1;
            if ((WizardPage)nextPage == WizardPage.BitmapScale && !NeedsBitmapScalePage()) {
                nextPage += 1;
            }

            GoToPage(nextPage);
            return Task.FromResult(false);
        }

        partial void OnCurrentPageIndexChanged(int value)
        {
            PreparePage((WizardPage)value);
            NotifyPagePropertiesChanged();
        }

        partial void OnEventTitleChanged(string value)
        {
            NotifyCanProceedChanged();
        }

        partial void OnSelectedMapTypeChanged(MapType value)
        {
            OnPropertyChanged(nameof(IsBitmapMap));
            OnPropertyChanged(nameof(IsPdfMap));
        }

        partial void OnMapFileNameChanged(string value)
        {
            OnPropertyChanged(nameof(HasMapFile));
        }

        partial void OnBitmapMapScaleTextChanged(string value)
        {
            NotifyCanProceedChanged();
        }

        partial void OnDpiTextChanged(string value)
        {
            NotifyCanProceedChanged();
        }

        partial void OnPrintScaleTextChanged(string value)
        {
            NotifyCanProceedChanged();
        }

        partial void OnUseMapDirectoryChanged(bool value)
        {
            OnPropertyChanged(nameof(UseOtherDirectory));
            NotifyCanProceedChanged();
        }

        partial void OnEventDirectoryChanged(string value)
        {
            NotifyCanProceedChanged();
        }

        partial void OnMapStandard2017Changed(bool value)
        {
            NotifyCanProceedChanged();
        }

        partial void OnMapStandardSpr2019Changed(bool value)
        {
            NotifyCanProceedChanged();
        }

        partial void OnMapStandard2000Changed(bool value)
        {
            NotifyCanProceedChanged();
        }

        partial void OnDescriptionStandard2018Changed(bool value)
        {
            NotifyCanProceedChanged();
        }

        partial void OnDescriptionStandard2004Changed(bool value)
        {
            NotifyCanProceedChanged();
        }

        private void SetMapFile(string fileName)
        {
            MapFileName = fileName;
            IsMapFileInfoVisible = false;
            IsMapFileErrorVisible = false;
            MapFileErrorMessage = "";

            string errorMessageText;
            float dpi;
            float validatedMapScale;
            MapType validatedMapType;
            int? validatedLowerPurpleMapLayer;
            Size validatedBitmapSize;
            RectangleF validatedMapBounds;

            if (CoreMapUtil.ValidateMapFile(fileName, out validatedMapScale, out dpi, out validatedBitmapSize, out validatedMapBounds, out validatedMapType, out validatedLowerPurpleMapLayer, out errorMessageText)) {
                mapScale = validatedMapScale;
                bitmapDpi = dpi;
                SelectedMapType = validatedMapType;
                bitmapSize = validatedBitmapSize;
                mapBounds = validatedMapBounds;
                lowerPurpleMapLayer = validatedLowerPurpleMapLayer;
                IsMapFileInfoVisible = true;

                if (NeedsBitmapScalePage()) {
                    BitmapMapScaleText = "15000";
                    DpiText = SelectedMapType == MapType.Bitmap && dpi > 0
                        ? dpi.ToString(CultureInfo.CurrentCulture)
                        : "";
                }
                else {
                    DpiText = "";
                }
            }
            else {
                mapScale = 0;
                bitmapDpi = 0;
                SelectedMapType = MapType.None;
                bitmapSize = new Size();
                mapBounds = RectangleF.Empty;
                lowerPurpleMapLayer = null;
                MapFileErrorMessage = errorMessageText;
                IsMapFileErrorVisible = true;
            }

            paperSizeInitialized = false;
            defaultPrintScale = 0;
            NotifyCanProceedChanged();
            OnPropertyChanged(nameof(MapScaleDisplay));
        }

        private void GoToPage(int pageIndex)
        {
            CurrentPageIndex = Math.Max((int)WizardPage.Title, Math.Min((int)WizardPage.Final, pageIndex));
        }

        private void PreparePage(WizardPage page)
        {
            if (page == WizardPage.PrintScale) {
                if (defaultPrintScale == 0) {
                    defaultPrintScale = mapScale;
                }

                PopulatePrintScales(mapScale);
                PrintScaleText = defaultPrintScale > 0 ? defaultPrintScale.ToString(CultureInfo.CurrentCulture) : "";
                OnPropertyChanged(nameof(MapScaleDisplay));
            }
            else if (page == WizardPage.PaperSize) {
                PreparePaperSize();
            }
            else if (page == WizardPage.Final) {
                IsFinalErrorVisible = false;
                FinalErrorMessage = "";
                OnPropertyChanged(nameof(EventFileFullPath));
            }
        }

        private void PreparePaperSize()
        {
            if (paperSizeInitialized) {
                return;
            }

            RectangleF printArea = mapBounds;
            float printScaleRatio = defaultPrintScale / mapScale;
            int preparedPageWidth;
            int preparedPageHeight;
            int preparedPageMargin;
            bool preparedLandscape;

            if (!printArea.IsEmpty && (SelectedMapType == MapType.PDF || SelectedMapType == MapType.Bitmap)) {
                MapUtil.GetExactPageSize(printArea, printScaleRatio, out preparedPageWidth, out preparedPageHeight, out preparedLandscape);
                preparedPageMargin = 0;
            }
            else {
                CoreMapUtil.GetDefaultPageSize(printArea, printScaleRatio, out preparedPageWidth, out preparedPageHeight, out preparedPageMargin, out preparedLandscape);
            }

            PageWidth = preparedPageWidth;
            PageHeight = preparedPageHeight;
            PageMargins = preparedPageMargin;
            PageLandscape = preparedLandscape;
            paperSizeInitialized = true;
        }

        private void PopulatePrintScales(float scale)
        {
            AvailablePrintScales.Clear();
            foreach (float printScale in MapUtil.PrintScaleList(scale)) {
                AvailablePrintScales.Add(printScale.ToString(CultureInfo.CurrentCulture));
            }
        }

        private bool CanProceedFromCurrentPage()
        {
            WizardPage page = (WizardPage)CurrentPageIndex;
            switch (page) {
            case WizardPage.Title:
                return EventTitle.Length > 0;

            case WizardPage.MapFile:
                return MapFileName.Length > 0 && !IsMapFileErrorVisible && SelectedMapType != MapType.None;

            case WizardPage.BitmapScale:
                return ApplyBitmapScale();

            case WizardPage.PrintScale:
                return ApplyPrintScale();

            case WizardPage.Directory:
                return UseMapDirectory || EventDirectory.Length > 0;

            case WizardPage.Standards:
                return (MapStandard2017 || MapStandardSpr2019 || MapStandard2000) &&
                       (DescriptionStandard2018 || DescriptionStandard2004);

            default:
                return true;
            }
        }

        private bool ApplyBitmapScale()
        {
            bool scaleOk = float.TryParse(BitmapMapScaleText, NumberStyles.Float, CultureInfo.CurrentCulture, out float parsedMapScale);
            if (!scaleOk || parsedMapScale <= 0) {
                return false;
            }

            if (SelectedMapType == MapType.Bitmap) {
                bool dpiOk = float.TryParse(DpiText, NumberStyles.Float, CultureInfo.CurrentCulture, out float parsedDpi);
                if (!dpiOk || parsedDpi <= 0) {
                    return false;
                }

                bitmapDpi = parsedDpi;
            }
            else {
                bitmapDpi = 0;
            }

            mapScale = parsedMapScale;
            paperSizeInitialized = false;
            return true;
        }

        private bool ApplyPrintScale()
        {
            bool result = float.TryParse(PrintScaleText, NumberStyles.Float, CultureInfo.CurrentCulture, out float parsedPrintScale);
            if (result && parsedPrintScale > 0) {
                defaultPrintScale = parsedPrintScale;
                paperSizeInitialized = false;
                return true;
            }

            return false;
        }

        private bool NeedsBitmapScalePage()
        {
            return SelectedMapType == MapType.Bitmap || SelectedMapType == MapType.PDF;
        }

        private string GetEventFullPath()
        {
            string directory = GetEventDirectory();
            string eventFileName = Util.FilterInvalidPathChars(EventTitle) + ".ppen";
            return Path.Combine(directory, eventFileName);
        }

        private string GetEventDirectory()
        {
            if (UseMapDirectory) {
                string? mapDirectory = Path.GetDirectoryName(MapFileName);
                return string.IsNullOrEmpty(mapDirectory) ? Environment.CurrentDirectory : mapDirectory;
            }

            return EventDirectory.Length > 0 ? EventDirectory : Environment.CurrentDirectory;
        }

        private string? GetInitialDirectoryForFolderPicker()
        {
            if (EventDirectory.Length > 0) {
                return EventDirectory;
            }

            string? mapDirectory = Path.GetDirectoryName(MapFileName);
            return string.IsNullOrEmpty(mapDirectory) ? null : mapDirectory;
        }

        private void SetCreateInfo()
        {
            Controller.CreateEventInfo info = new Controller.CreateEventInfo();
            info.title = EventTitle;
            info.eventFileName = GetEventFullPath();
            info.mapType = SelectedMapType;
            info.mapFileName = MapFileName;
            info.scale = mapScale;
            info.allControlsPrintScale = defaultPrintScale;
            info.dpi = SelectedMapType == MapType.Bitmap ? bitmapDpi : 0;
            info.firstCode = StartingCode;
            info.disallowInvertibleCodes = DisallowInvertibleCodes;
            info.descriptionLangId = null;
            info.mapStandard = GetMapStandard();
            info.descriptionStandard = DescriptionStandard2018 ? "2018" : "2004";

            if (info.mapType == MapType.OCAD && lowerPurpleMapLayer != null) {
                info.blend = PurpleColorBlend.UpperLowerPurple;
                info.lowerPurpleLayer = lowerPurpleMapLayer;
            }
            else {
                info.blend = PurpleColorBlend.Blend;
                info.lowerPurpleLayer = null;
            }

            PrintArea printArea = new PrintArea();
            printArea.autoPrintArea = true;
            printArea.restrictToPageSize = true;
            printArea.pageWidth = PageWidth;
            printArea.pageHeight = PageHeight;
            printArea.pageMargins = PageMargins;
            printArea.pageLandscape = PageLandscape;
            info.printArea = printArea;

            CreateEventInfo = info;
        }

        private string GetMapStandard()
        {
            if (MapStandard2017) {
                return "2017";
            }

            if (MapStandardSpr2019) {
                return "Spr2019";
            }

            return "2000";
        }

        private bool TryCreateEvent(out string errorMessageText)
        {
            string? directoryName = Path.GetDirectoryName(CreateEventInfo.eventFileName);
            string directory = string.IsNullOrEmpty(directoryName) ? Environment.CurrentDirectory : directoryName;
            if (!Directory.Exists(directory)) {
                try {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception exception) {
                    errorMessageText = string.Format(MiscText.CannotCreateDirectory, directory) + "\r\n" + exception.Message;
                    return false;
                }
            }

            if (File.Exists(CreateEventInfo.eventFileName)) {
                errorMessageText = string.Format(MiscText.FileAlreadyExists, Path.GetFileName(CreateEventInfo.eventFileName));
                return false;
            }

            byte[] bytes = { 0 };
            try {
                File.WriteAllBytes(CreateEventInfo.eventFileName, bytes);
            }
            catch (Exception exception) {
                errorMessageText = string.Format(MiscText.CannotCreateFile, Path.GetFileName(CreateEventInfo.eventFileName)) + "\r\n" + exception.Message;
                return false;
            }

            errorMessageText = "";
            return true;
        }

        private void NotifyCanProceedChanged()
        {
            OnPropertyChanged(nameof(IsNextEnabled));
            OnPropertyChanged(nameof(EventFileFullPath));
        }

        private void NotifyPagePropertiesChanged()
        {
            OnPropertyChanged(nameof(IsTitlePage));
            OnPropertyChanged(nameof(IsMapFilePage));
            OnPropertyChanged(nameof(IsBitmapScalePage));
            OnPropertyChanged(nameof(IsPrintScalePage));
            OnPropertyChanged(nameof(IsPaperSizePage));
            OnPropertyChanged(nameof(IsDirectoryPage));
            OnPropertyChanged(nameof(IsStandardsPage));
            OnPropertyChanged(nameof(IsNumberingPage));
            OnPropertyChanged(nameof(IsFinalPage));
            OnPropertyChanged(nameof(IsNotFinalPage));
            OnPropertyChanged(nameof(IsBackEnabled));
            OnPropertyChanged(nameof(IsNextEnabled));
        }
    }
}

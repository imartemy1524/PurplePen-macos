// MainWindow.axaml.cs
//
// Code-behind for the main window. Handles UI events that need
// direct window interaction (like showing modal dialogs), which
// don't fit cleanly into the ViewModel layer.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using AvUtil;
using PurplePen;
using PurplePen.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using Key = Avalonia.Input.Key;
using KeyGesture = Avalonia.Input.KeyGesture;
using KeyModifiers = Avalonia.Input.KeyModifiers;

namespace AvPurplePen.Views
{
    /// <summary>
    /// The main application window.
    /// </summary>
    public partial class MainWindow : Window
    {
        private MousePointerShape _mousePointerShape = new MousePointerShape(PredefinedMousePointerShape.Arrow);
        private MainWindowViewModel? _mainViewModel;
        private bool _updatingTopologyScrollBar;
        private NativeMenuItem? _openRecentMenu;

        // Has the MousePointerShape that should be used in the map viewer.
        public static readonly DirectProperty<MainWindow, MousePointerShape> MapMousePointerShapeProperty =
                AvaloniaProperty.RegisterDirect<MainWindow, MousePointerShape>(
                    nameof(MapMousePointerShape),
                    getter: o => o.MapMousePointerShape,
                    setter: (o, value) => o.MapMousePointerShape = value);

        /// <summary>
        /// Initializes the main window and its components.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContextChanged += MainWindow_DataContextChanged;
            ApplicationIdleService.ApplicationIdle += ApplicationIdle;
        }

        private void MainWindow_DataContextChanged(object? sender, EventArgs e)
        {
            if (_mainViewModel != null) {
                _mainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
            }

            _mainViewModel = DataContext as MainWindowViewModel;

            if (_mainViewModel != null) {
                _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
                SetupMacOSNativeMenu();
            }

            UpdateDescriptionTopologyVisibility();
        }

        private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.IsTopologyViewVisible)) {
                UpdateDescriptionTopologyVisibility();
            }
            else if (e.PropertyName == nameof(MainWindowViewModel.IsTopologyViewEnabled)) {
                UpdateDescriptionTopologyVisibility();
            }
            else if (e.PropertyName == nameof(MainWindowViewModel.TopologyVersion)) {
                FitTopologyMapViewerToWidth();
            }
            else if (e.PropertyName == nameof(MainWindowViewModel.RecentFiles)) {
                UpdateRecentFilesMenu();
            }
        }

        private void UpdateDescriptionTopologyVisibility()
        {
            if (_mainViewModel == null) {
                return;
            }

            bool showTopology = _mainViewModel.IsTopologyViewVisible && _mainViewModel.IsTopologyViewEnabled;
            descriptionViewer.IsVisible = !showTopology;
            panelTopology.IsVisible = showTopology;
            radioButtonDescriptions.IsChecked = !showTopology;
            radioButtonTopology.IsChecked = showTopology;
            radioButtonTopology.IsEnabled = _mainViewModel.IsTopologyViewEnabled;

            if (showTopology) {
                FitTopologyMapViewerToWidth();
            }
        }

        // Setup macOS native menu bar
        private void SetupMacOSNativeMenu()
        {
            try {
                if (_mainViewModel == null)
                    return;

                var nativeMenu = new NativeMenu();
                nativeMenu.Items.Add(BuildFileMenu());
                nativeMenu.Items.Add(BuildEditMenu());
                nativeMenu.Items.Add(BuildViewMenu());
                nativeMenu.Items.Add(BuildAddMenu());
                nativeMenu.Items.Add(BuildEventMenu());
                nativeMenu.Items.Add(BuildCourseMenu());
                nativeMenu.Items.Add(BuildItemMenu());
                nativeMenu.Items.Add(BuildReportsMenu());
                nativeMenu.Items.Add(BuildHelpMenu());

                NativeMenu.SetMenu(this, nativeMenu);

                // Store reference to Open Recent menu for updates
                _openRecentMenu = nativeMenu.Items[0] is NativeMenuItem fileMenu
                    ? fileMenu.Menu?.Items.FirstOrDefault(i => i is NativeMenuItem mi && mi.Header?.ToString() == "Open Recent") as NativeMenuItem
                    : null;

                UpdateRecentFilesMenu();
            }
            catch (Exception) {
                // If native menu setup fails, just continue
            }
        }

        private NativeMenuItem BuildFileMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = "New Event", Command = _mainViewModel!.NewEventCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Open...", Command = _mainViewModel!.FileOpenPurplePenFileCommand, Gesture = new KeyGesture(Key.O, KeyModifiers.Meta) });

            // Add Open Recent menu
            var recentMenu = new NativeMenu();
            _openRecentMenu = new NativeMenuItem { Header = "Open Recent", Menu = recentMenu };
            menu.Items.Add(_openRecentMenu);

            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Save", Command = _mainViewModel!.SaveCommand, Gesture = new KeyGesture(Key.S, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = "Save As...", Command = _mainViewModel!.SaveAsCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Create OCAD Files...", Command = _mainViewModel!.CreateOcadFilesCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Create Image Files...", Command = _mainViewModel!.CreateImageFilesCommand });

            var pdfMenu = new NativeMenu();
            pdfMenu.Items.Add(new NativeMenuItem { Header = "Description PDF", Command = _mainViewModel!.CreateDescriptionPdfCommand });
            pdfMenu.Items.Add(new NativeMenuItem { Header = "Punchcard PDF", Command = _mainViewModel!.CreatePunchcardPdfCommand });
            pdfMenu.Items.Add(new NativeMenuItem { Header = "Course PDF", Command = _mainViewModel!.CreateCoursePdfCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Create PDFs", Menu = pdfMenu });

            var routeMenu = new NativeMenu();
            routeMenu.Items.Add(new NativeMenuItem { Header = "Create Route Gadget Files", Command = _mainViewModel!.CreateRouteGadgetFilesCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Create Route Review Files", Menu = routeMenu });

            menu.Items.Add(new NativeMenuItem { Header = "Create XML", Command = _mainViewModel!.CreateXmlCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Create GPX File", Command = _mainViewModel!.CreateGpxCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Create KML File", Command = _mainViewModel!.CreateKmlFilesCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Print Descriptions", Command = _mainViewModel!.PrintDescriptionsCommand, Gesture = new KeyGesture(Key.P, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = "Print Punch Cards", Command = _mainViewModel!.PrintPunchCardsCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Print Courses", Command = _mainViewModel!.PrintCoursesCommand });

            var printAreaMenu = new NativeMenu();
            printAreaMenu.Items.Add(new NativeMenuItem { Header = "This Part", Command = _mainViewModel!.SetPrintAreaThisPartCommand });
            printAreaMenu.Items.Add(new NativeMenuItem { Header = "This Course", Command = _mainViewModel!.SetPrintAreaThisCourseCommand });
            printAreaMenu.Items.Add(new NativeMenuItem { Header = "All Courses", Command = _mainViewModel!.SetPrintAreaAllCoursesCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Set Print Area", Menu = printAreaMenu });

            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Program Language", Command = _mainViewModel!.ShowSwitchLanguageDialogCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Exit", Command = _mainViewModel!.ExitCommand });
            return new NativeMenuItem { Header = "File", Menu = menu };
        }

        private NativeMenuItem BuildEditMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = "Cancel", Command = _mainViewModel!.CancelCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Undo", Command = _mainViewModel!.UndoCommand, Gesture = new KeyGesture(Key.Z, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = "Redo", Command = _mainViewModel!.RedoCommand, Gesture = new KeyGesture(Key.Z, KeyModifiers.Meta | KeyModifiers.Shift) });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Delete", Command = _mainViewModel!.DeleteSelectionCommand, Gesture = new KeyGesture(Key.Back, KeyModifiers.Meta) });
            return new NativeMenuItem { Header = "Edit", Menu = menu };
        }

        private NativeMenuItem BuildViewMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = "Entire Course", Command = _mainViewModel!.ViewEntireCourseCommand, Gesture = new KeyGesture(Key.F2) });
            menu.Items.Add(new NativeMenuItem { Header = "Entire Map", Command = _mainViewModel!.ViewEntireMapCommand, Gesture = new KeyGesture(Key.F3) });

            var zoomMenu = new NativeMenu();
            zoomMenu.Items.Add(new NativeMenuItem { Header = "50%", Command = _mainViewModel!.SetZoomCommand, CommandParameter = 0.5 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = "100%", Command = _mainViewModel!.SetZoomCommand, CommandParameter = 1.0 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = "150%", Command = _mainViewModel!.SetZoomCommand, CommandParameter = 1.5 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = "200%", Command = _mainViewModel!.SetZoomCommand, CommandParameter = 2.0 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = "300%", Command = _mainViewModel!.SetZoomCommand, CommandParameter = 3.0 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = "500%", Command = _mainViewModel!.SetZoomCommand, CommandParameter = 5.0 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = "1000%", Command = _mainViewModel!.SetZoomCommand, CommandParameter = 10.0 });
            menu.Items.Add(new NativeMenuItem { Header = "Zoom", Menu = zoomMenu });

            var intensityMenu = new NativeMenu();
            intensityMenu.Items.Add(new NativeMenuItem { Header = "Very Low", Command = _mainViewModel!.SetMapIntensityCommand, CommandParameter = 0.2 });
            intensityMenu.Items.Add(new NativeMenuItem { Header = "Low", Command = _mainViewModel!.SetMapIntensityCommand, CommandParameter = 0.4 });
            intensityMenu.Items.Add(new NativeMenuItem { Header = "Medium", Command = _mainViewModel!.SetMapIntensityCommand, CommandParameter = 0.6 });
            intensityMenu.Items.Add(new NativeMenuItem { Header = "High", Command = _mainViewModel!.SetMapIntensityCommand, CommandParameter = 0.8 });
            intensityMenu.Items.Add(new NativeMenuItem { Header = "Full", Command = _mainViewModel!.SetMapIntensityCommand, CommandParameter = 1.0 });
            menu.Items.Add(new NativeMenuItem { Header = "Map Intensity", Menu = intensityMenu });

            var qualityMenu = new NativeMenu();
            qualityMenu.Items.Add(new NativeMenuItem { Header = "Normal", Command = _mainViewModel!.SetNormalQualityCommand });
            qualityMenu.Items.Add(new NativeMenuItem { Header = "High", Command = _mainViewModel!.SetHighQualityCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Map Quality", Menu = qualityMenu });

            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Show Print Area", Command = _mainViewModel!.ToggleShowPrintAreaCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Show Popups", Command = _mainViewModel!.ToggleShowPopupsCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "All Controls", Command = _mainViewModel!.ToggleAllControlsCommand, Gesture = new KeyGesture(Key.F4) });
            menu.Items.Add(new NativeMenuItem { Header = "Other Courses", Command = _mainViewModel!.ShowOtherCoursesCommand, Gesture = new KeyGesture(Key.F5) });
            menu.Items.Add(new NativeMenuItem { Header = "Clear Other Courses", Command = _mainViewModel!.ClearOtherCoursesCommand, Gesture = new KeyGesture(Key.F5, KeyModifiers.Shift) });
            return new NativeMenuItem { Header = "View", Menu = menu };
        }

        private NativeMenuItem BuildAddMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = "Start", Command = _mainViewModel!.AddStartCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Control", Command = _mainViewModel!.AddControlCommand, Gesture = new KeyGesture(Key.A, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = "Finish", Command = _mainViewModel!.AddFinishCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Descriptions", Command = _mainViewModel!.AddDescriptionsCommand });

            var mapExchangeMenu = new NativeMenu();
            mapExchangeMenu.Items.Add(new NativeMenuItem { Header = "Map Flip", Command = _mainViewModel!.AddMapFlipControlCommand });
            mapExchangeMenu.Items.Add(new NativeMenuItem { Header = "Map Exchange Control", Command = _mainViewModel!.AddMapExchangeControlCommand });
            mapExchangeMenu.Items.Add(new NativeMenuItem { Header = "Map Exchange Separate", Command = _mainViewModel!.AddMapExchangeSeparateCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Map Exchange", Menu = mapExchangeMenu });

            menu.Items.Add(new NativeMenuItem { Header = "Variation", Command = _mainViewModel!.AddVariationCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Text Line", Command = _mainViewModel!.AddTextLineCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Map Issue", Command = _mainViewModel!.AddMapIssueCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Mandatory Crossing", Command = _mainViewModel!.AddMandatoryCrossingCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Optional Crossing", Command = _mainViewModel!.AddOptCrossingCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Out of Bounds", Command = _mainViewModel!.AddOutOfBoundsCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Dangerous", Command = _mainViewModel!.AddDangerousCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Construction", Command = _mainViewModel!.AddConstructionCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Water", Command = _mainViewModel!.AddWaterCommand });
            menu.Items.Add(new NativeMenuItem { Header = "First Aid", Command = _mainViewModel!.AddFirstAidCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Forbidden", Command = _mainViewModel!.AddForbiddenCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Boundary", Command = _mainViewModel!.AddBoundaryCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Registration Mark", Command = _mainViewModel!.AddRegMarkCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "White Out", Command = _mainViewModel!.AddWhiteOutCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Text", Command = _mainViewModel!.AddTextCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Image", Command = _mainViewModel!.AddImageCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Line", Command = _mainViewModel!.AddLineCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Rectangle", Command = _mainViewModel!.AddRectangleCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Ellipse", Command = _mainViewModel!.AddEllipseCommand });
            return new NativeMenuItem { Header = "Add", Menu = menu };
        }

        private NativeMenuItem BuildEventMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = "Change Map File", Command = _mainViewModel!.ChangeMapFileCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Change Codes", Command = _mainViewModel!.ChangeCodesCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Auto Numbering", Command = _mainViewModel!.AutoNumberingCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Remove Unused Controls", Command = _mainViewModel!.RemoveUnusedControlsCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Move All Controls", Command = _mainViewModel!.MoveAllControlsCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Punch Patterns", Command = _mainViewModel!.PunchPatternsCommand });
            menu.Items.Add(new NativeMenuItemSeparator());

            var iofMenu = new NativeMenu();
            iofMenu.Items.Add(new NativeMenuItem { Header = "Description Std 2004", Command = _mainViewModel!.SetDescriptionStd2004Command });
            iofMenu.Items.Add(new NativeMenuItem { Header = "Description Std 2018", Command = _mainViewModel!.SetDescriptionStd2018Command });
            iofMenu.Items.Add(new NativeMenuItemSeparator());
            iofMenu.Items.Add(new NativeMenuItem { Header = "Map Std 2000", Command = _mainViewModel!.SetMapStd2000Command });
            iofMenu.Items.Add(new NativeMenuItem { Header = "Map Std 2017", Command = _mainViewModel!.SetMapStd2017Command });
            iofMenu.Items.Add(new NativeMenuItem { Header = "Map Std 2019", Command = _mainViewModel!.SetMapStdSpr2019Command });
            menu.Items.Add(new NativeMenuItem { Header = "IOF Standards", Menu = iofMenu });

            menu.Items.Add(new NativeMenuItem { Header = "Customize Descriptions", Command = _mainViewModel!.CustomizeDescriptionsCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Customize Course Appearance", Command = _mainViewModel!.CustomizeCourseAppearanceCommand });
            return new NativeMenuItem { Header = "Event", Menu = menu };
        }

        private NativeMenuItem BuildCourseMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = "Add Course", Command = _mainViewModel!.ShowAddCourseDialogCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Delete Course", Command = _mainViewModel!.DeleteCourseCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Duplicate Course", Command = _mainViewModel!.DuplicateCourseCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Properties", Command = _mainViewModel!.ShowCoursePropertiesCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Course Order", Command = _mainViewModel!.ShowCourseOrderCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Course Load", Command = _mainViewModel!.ShowCourseLoadCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Course Variation Report", Command = _mainViewModel!.ShowCourseVariationReportCommand });
            return new NativeMenuItem { Header = "Course", Menu = menu };
        }

        private NativeMenuItem BuildItemMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = "Delete", Command = _mainViewModel!.DeleteSelectionCommand, Gesture = new KeyGesture(Key.Back, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = "Delete Fork", Command = _mainViewModel!.DeleteForkCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Add Bend", Command = _mainViewModel!.AddBendCommand, Gesture = new KeyGesture(Key.B, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = "Remove Bend", Command = _mainViewModel!.RemoveBendCommand, Gesture = new KeyGesture(Key.B, KeyModifiers.Meta | KeyModifiers.Shift) });
            menu.Items.Add(new NativeMenuItem { Header = "Add Gap", Command = _mainViewModel!.AddGapCommand, Gesture = new KeyGesture(Key.G, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = "Remove Gap", Command = _mainViewModel!.RemoveGapCommand, Gesture = new KeyGesture(Key.G, KeyModifiers.Meta | KeyModifiers.Shift) });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Change Text", Command = _mainViewModel!.ChangeTextCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Change Line Appearance", Command = _mainViewModel!.ChangeLineAppearanceCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Rotate", Command = _mainViewModel!.RotateCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Stretch", Command = _mainViewModel!.StretchCommand });

            var flaggingMenu = new NativeMenu();
            flaggingMenu.Items.Add(new NativeMenuItem { Header = "No Flagging", Command = _mainViewModel!.SetNoFlaggingCommand });
            flaggingMenu.Items.Add(new NativeMenuItem { Header = "Entire Flagging", Command = _mainViewModel!.SetEntireFlaggingCommand });
            flaggingMenu.Items.Add(new NativeMenuItem { Header = "Begin Flagging", Command = _mainViewModel!.SetBeginFlaggingCommand });
            flaggingMenu.Items.Add(new NativeMenuItem { Header = "End Flagging", Command = _mainViewModel!.SetEndFlaggingCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Leg Flagging", Menu = flaggingMenu });

            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Change Displayed Courses", Command = _mainViewModel!.ChangeDisplayedCoursesCommand });
            return new NativeMenuItem { Header = "Item", Menu = menu };
        }

        private NativeMenuItem BuildReportsMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = "Course Summary", Command = _mainViewModel!.ShowCourseSummaryCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Event Audit", Command = _mainViewModel!.ShowEventAuditCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Leg Lengths", Command = _mainViewModel!.ShowLegLengthsCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Control Crossref", Command = _mainViewModel!.ShowControlCrossrefCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Control and Leg Load", Command = _mainViewModel!.ShowControlAndLegLoadCommand });
            return new NativeMenuItem { Header = "Reports", Menu = menu };
        }

        private NativeMenuItem BuildHelpMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = "Help Contents", Command = _mainViewModel!.HelpContentsCommand, Gesture = new KeyGesture(Key.F1) });
            menu.Items.Add(new NativeMenuItem { Header = "Help Translated", Command = _mainViewModel!.HelpTranslatedCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "Main Web Site", Command = _mainViewModel!.OpenMainWebSiteCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Support Web Site", Command = _mainViewModel!.OpenSupportWebSiteCommand });
            menu.Items.Add(new NativeMenuItem { Header = "Donate Web Site", Command = _mainViewModel!.OpenDonateWebSiteCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = "About", Command = _mainViewModel!.ShowAboutDialogCommand });
            return new NativeMenuItem { Header = "Help", Menu = menu };
        }

        // Updates the "Open Recent" menu with recent files
        private void UpdateRecentFilesMenu()
        {
            if (_mainViewModel == null || _openRecentMenu?.Menu == null)
                return;

            _openRecentMenu.Menu.Items.Clear();

            // If no recent files, show a disabled placeholder
            if (_mainViewModel.RecentFiles.Count == 0) {
                var placeholderItem = new NativeMenuItem { Header = "(No recent files)", IsEnabled = false };
                _openRecentMenu.Menu.Items.Add(placeholderItem);
                return;
            }

            // Get the currently open file
            string currentFile = _mainViewModel.CurrentFileName ?? "";

            // Add each recent file
            foreach (var filePath in _mainViewModel.RecentFiles) {
                // Extract event title from the .ppen file (only reads first 10KB)
                string eventTitle = UserSettings.GetEventTitle(filePath);

                // Mark the currently open file as checked
                bool isCurrentFile = !string.IsNullOrEmpty(currentFile) &&
                                    System.IO.Path.GetFullPath(filePath).Equals(
                                        System.IO.Path.GetFullPath(currentFile),
                                        StringComparison.OrdinalIgnoreCase);

                var menuItem = new NativeMenuItem {
                    Header = eventTitle,
                    Command = _mainViewModel.OpenRecentFileCommand,
                    CommandParameter = filePath,
                    IsChecked = isCurrentFile
                };
                _openRecentMenu.Menu.Items.Add(menuItem);
            }
        }

        public MousePointerShape MapMousePointerShape {
            get => _mousePointerShape;
            set {
                _mousePointerShape = value;
                mapViewer.Cursor = Cursors.CursorFromMousePointerShape(value);
            }
        }

        private void RadioButtonDescriptions_Checked(object? sender, RoutedEventArgs e)
        {
            if (_mainViewModel != null) {
                _mainViewModel.IsTopologyViewVisible = false;
            }
        }

        private void RadioButtonTopology_Checked(object? sender, RoutedEventArgs e)
        {
            if (_mainViewModel != null && _mainViewModel.IsTopologyViewEnabled) {
                _mainViewModel.IsTopologyViewVisible = true;
            }
        }

        // Mouse activity in the main map viewer.
        private async void MapViewer_MouseActivity(object? sender, MapViewer.FancyMouseEventArgs e)
        {
            await HandleMapViewerMouseActivity(e, false);
        }

        private async void TopologyMapViewer_MouseActivity(object? sender, MapViewer.FancyMouseEventArgs e)
        {
            await HandleMapViewerMouseActivity(e, true);
        }

        private async System.Threading.Tasks.Task HandleMapViewerMouseActivity(MapViewer.FancyMouseEventArgs e, bool topology)
        {
            MainWindowViewModel? vm = this.DataContext as MainWindowViewModel;
            if (vm == null)
                return;

            // Only left and right buttons have meaning (except for move)
            if (e.Button != MouseButton.Left && e.Button != MouseButton.Right && e.FancyAction != MapViewer.FancyMouseAction.Move)
                return;

            bool isRightButton = (e.Button == MouseButton.Right);
            PointF location = Conv.ToPointF(e.WorldLocation);
            PointF locationStart = Conv.ToPointF(e.WorldDragStart);
            MapViewer activeMapViewer = topology ? mapViewerTopology : mapViewer;
            float pixelSize = activeMapViewer.PixelSize;
            DragAction dragAction = DragAction.None;
            
            switch (e.FancyAction) {
            case MapViewer.FancyMouseAction.Move:
#if PORTING
                // Do we need to deal with leave here to report outside the viewport?
#endif
                if (topology)
                    vm.TopologyMapViewerMouseMove(location, pixelSize);
                else
                    vm.MapViewerMouseMove(location, pixelSize);
                break;

            case MapViewer.FancyMouseAction.Down:
                if (topology)
                    dragAction = isRightButton ? vm.TopologyMapViewerRightButtonDown(location, pixelSize) : vm.TopologyMapViewerLeftButtonDown(location, pixelSize);
                else
                    dragAction = isRightButton ? vm.MapViewerRightButtonDown(location, pixelSize) : vm.MapViewerLeftButtonDown(location, pixelSize);
                break;

            case MapViewer.FancyMouseAction.Drag:
                if (topology) {
                    if (isRightButton)
                        vm.TopologyMapViewerRightButtonDrag(location, locationStart, pixelSize);
                    else
                        vm.TopologyMapViewerLeftButtonDrag(location, locationStart, pixelSize);
                }
                else if (isRightButton)
                    vm.MapViewerRightButtonDrag(location, locationStart, pixelSize);
                else
                    vm.MapViewerLeftButtonDrag(location, locationStart, pixelSize);
                break;

            case MapViewer.FancyMouseAction.Up:
                if (topology) {
                    if (isRightButton)
                        vm.TopologyMapViewerRightButtonUp(location, pixelSize);
                    else
                        vm.TopologyMapViewerLeftButtonUp(location, pixelSize);
                }
                else if (isRightButton) 
                    vm.MapViewerRightButtonUp(location, pixelSize);
                else
                    vm.MapViewerLeftButtonUp(location, pixelSize);
                break;

            case MapViewer.FancyMouseAction.DragEnd:
                if (topology) {
                    if (isRightButton)
                        await vm.TopologyMapViewerRightButtonEndDrag(location, locationStart, pixelSize);
                    else
                        await vm.TopologyMapViewerLeftButtonEndDrag(location, locationStart, pixelSize);
                }
                else if (isRightButton)
                    await vm.MapViewerRightButtonEndDrag(location, locationStart, pixelSize);
                else
                    await vm.MapViewerLeftButtonEndDrag(location, locationStart, pixelSize);
                break;

            case MapViewer.FancyMouseAction.Click:
                if (topology) {
                    if (isRightButton)
                        await vm.TopologyMapViewerRightButtonClick(location, pixelSize);
                    else
                        await vm.TopologyMapViewerLeftButtonClick(location, pixelSize);
                }
                else if (isRightButton)
                    await vm.MapViewerRightButtonClick(location, pixelSize);
                else
                    await vm.MapViewerLeftButtonClick(location, pixelSize);
                break;

            case MapViewer.FancyMouseAction.DragCancel:
                if (topology) {
                    if (isRightButton)
                        vm.TopologyMapViewerRightButtonCancelDrag();
                    else
                        vm.TopologyMapViewerLeftButtonCancelDrag();
                }
                else if (isRightButton)
                    vm.MapViewerRightButtonCancelDrag();
                else
                    vm.MapViewerLeftButtonCancelDrag();
                break;

            case MapViewer.FancyMouseAction.Hover:
#if !PORTING
                // handle hover
#endif
                break;

            default:
                break;
            }

            switch (dragAction) {
            case DragAction.None:
                e.MouseDownResult = MapViewer.MouseDownResult.None; break;
            case DragAction.SuppressClick:
                e.MouseDownResult = MapViewer.MouseDownResult.SuppressClick; break;
            case DragAction.MapDrag:
                e.MouseDownResult = MapViewer.MouseDownResult.ImmediatePan;  break;
            case DragAction.ImmediateDrag:
                e.MouseDownResult = MapViewer.MouseDownResult.ImmediateDrag; break;
            case DragAction.DelayedDrag:
                e.MouseDownResult = MapViewer.MouseDownResult.DelayedDrag; break;
            default:
                break;
            }
        }

        private void TopologyMapViewer_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            FitTopologyMapViewerToWidth();
        }

        private void TopologyMapViewer_ViewportChanged(object? sender, PanAndZoom.ViewportChangedEventArgs e)
        {
            UpdateTopologyScrollBar();
        }

        private void TopologyScrollBar_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_updatingTopologyScrollBar) {
                mapViewerTopology.VScrollValue = (int)Math.Round(e.NewValue);
            }
        }

        private void FitTopologyMapViewerToWidth()
        {
            if (_mainViewModel == null || !_mainViewModel.IsTopologyViewEnabled || _mainViewModel.TopologyMapDisplay == null) {
                UpdateTopologyScrollBar();
                return;
            }

            float worldWidth = _mainViewModel.TopologyMapDisplay.Bounds.Width;
            if (worldWidth <= 0 || mapViewerTopology.Bounds.Width <= 0) {
                UpdateTopologyScrollBar();
                return;
            }

            int availableWidth = Math.Max(1, (int)Math.Round(mapViewerTopology.Bounds.Width));
            mapViewerTopology.ZoomFactor = mapViewerTopology.ZoomFactorForWorldWidth(availableWidth, worldWidth);
            mapViewerTopology.Recenter();
            UpdateTopologyScrollBar();
        }

        private void UpdateTopologyScrollBar()
        {
            if (topologyScrollBar == null || mapViewerTopology == null) {
                return;
            }

            _updatingTopologyScrollBar = true;
            try {
                if (mapViewerTopology.VScrollEnable) {
                    topologyScrollBar.IsVisible = true;
                    topologyScrollBar.SmallChange = mapViewerTopology.VScrollSmallChange;
                    topologyScrollBar.LargeChange = mapViewerTopology.VScrollLargeChange;
                    topologyScrollBar.ViewportSize = mapViewerTopology.VScrollLargeChange;
                    topologyScrollBar.Value = mapViewerTopology.VScrollValue;
                }
                else {
                    topologyScrollBar.IsVisible = false;
                    topologyScrollBar.Value = 0;
                }
            }
            finally {
                _updatingTopologyScrollBar = false;
            }
        }


        // This is called when the application becomes idle after processing input. We can use this to update
        // the UI in response to changes that may have occurred.
        private void ApplicationIdle(object? sender, System.EventArgs e)
        {
            if (this.IsVisible) {
                // The application is idle. If the application state has changed, update the
                // user interface to match.
                if (this.DataContext is MainWindowViewModel viewModel) {
                    viewModel.UpdateStateOnIdle();
                }
            }
        }

        // MoveAllControls button handlers
        private void ConfirmMoveAllControls_Click(object? sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel viewModel) {
                viewModel.ConfirmMoveAllControls();
            }
        }

        private void CancelMoveAllControls_Click(object? sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel viewModel) {
                viewModel.CancelMoveAllControls();
            }
        }
    }
}

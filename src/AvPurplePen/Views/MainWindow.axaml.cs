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
            LocalizedStringManager.Instance.LanguageChanged += LocalizedStringManager_LanguageChanged;
        }

        protected override void OnClosed(EventArgs e)
        {
            LocalizedStringManager.Instance.LanguageChanged -= LocalizedStringManager_LanguageChanged;
            base.OnClosed(e);
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

        private static string L(string key)
        {
            return UIText.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
        }

        private void LocalizedStringManager_LanguageChanged(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (OperatingSystem.IsMacOS()) {
                    SetupMacOSNativeMenu();
                }
            });
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
                    ? fileMenu.Menu?.Items.FirstOrDefault(i => i is NativeMenuItem mi && mi.Header?.ToString() == L("AvMainFrame_openRecentMenu_Text")) as NativeMenuItem
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
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_newEventMenu_Text"), Command = _mainViewModel!.NewEventCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_openMenu_Text"), Command = _mainViewModel!.FileOpenPurplePenFileCommand, Gesture = new KeyGesture(Key.O, KeyModifiers.Meta) });

            // Add Open Recent menu
            var recentMenu = new NativeMenu();
            _openRecentMenu = new NativeMenuItem { Header = L("AvMainFrame_openRecentMenu_Text"), Menu = recentMenu };
            menu.Items.Add(_openRecentMenu);

            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_saveMenu_Text"), Command = _mainViewModel!.SaveCommand, Gesture = new KeyGesture(Key.S, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_saveAsMenu_Text"), Command = _mainViewModel!.SaveAsCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createOcadFilesMenu_Text"), Command = _mainViewModel!.CreateOcadFilesCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createImageFilesMenu_Text"), Command = _mainViewModel!.CreateImageFilesCommand });

            var pdfMenu = new NativeMenu();
            pdfMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createDescriptionPdfMenu_Text"), Command = _mainViewModel!.CreateDescriptionPdfCommand });
            pdfMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createPunchcardPdfMenu_Text"), Command = _mainViewModel!.CreatePunchcardPdfCommand });
            pdfMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createCoursePdfMenu_Text"), Command = _mainViewModel!.CreateCoursePdfCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createPDFsMenu_Text"), Menu = pdfMenu });

            var routeMenu = new NativeMenu();
            routeMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createRouteGadgetFilesMenu_Text"), Command = _mainViewModel!.CreateRouteGadgetFilesCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createRouteReviewFilesToolStripMenuItem_Text"), Menu = routeMenu });

            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createXmlMenu_Text"), Command = _mainViewModel!.CreateXmlCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createGPXFileMenu_Text"), Command = _mainViewModel!.CreateGpxCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_createKMLFileMenu_Text"), Command = _mainViewModel!.CreateKmlFilesCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_printDescriptionsMenu_Text"), Command = _mainViewModel!.PrintDescriptionsCommand, Gesture = new KeyGesture(Key.P, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_printPunchCardsMenu_Text"), Command = _mainViewModel!.PrintPunchCardsCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_printCoursesMenu_Text"), Command = _mainViewModel!.PrintCoursesCommand });

            var printAreaMenu = new NativeMenu();
            printAreaMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_printAreaThisPartMenu_Text"), Command = _mainViewModel!.SetPrintAreaThisPartCommand });
            printAreaMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_printAreaThisCourseMenu_Text"), Command = _mainViewModel!.SetPrintAreaThisCourseCommand });
            printAreaMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_printAreaAllCoursesMenu_Text"), Command = _mainViewModel!.SetPrintAreaAllCoursesCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_setPrintAreaMenu_Text"), Menu = printAreaMenu });

            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem {
                Header = L("MainFrame_settingsMenu_Text"),
                Command = _mainViewModel!.ShowSettingsDialogCommand,
                Gesture = new KeyGesture(Key.OemComma, KeyModifiers.Meta)
            });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_exitMenu_Text"), Command = _mainViewModel!.ExitCommand });
            return new NativeMenuItem { Header = L("MainFrame_fileMenu_Text"), Menu = menu };
        }

        private NativeMenuItem BuildEditMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_cancelMenu_Text"), Command = _mainViewModel!.CancelCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_undoMenu_Text"), Command = _mainViewModel!.UndoCommand, Gesture = new KeyGesture(Key.Z, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_redoMenu_Text"), Command = _mainViewModel!.RedoCommand, Gesture = new KeyGesture(Key.Z, KeyModifiers.Meta | KeyModifiers.Shift) });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_deleteMenu_Text"), Command = _mainViewModel!.DeleteSelectionCommand, Gesture = new KeyGesture(Key.Back, KeyModifiers.Meta) });
            return new NativeMenuItem { Header = L("MainFrame_editMenu_Text"), Menu = menu };
        }

        private NativeMenuItem BuildViewMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_entireCourseMenu_Text"), Command = _mainViewModel!.ViewEntireCourseCommand, Gesture = new KeyGesture(Key.F2) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_entireMapMenu_Text"), Command = _mainViewModel!.ViewEntireMapCommand, Gesture = new KeyGesture(Key.F3) });

            var zoomMenu = new NativeMenu();
            zoomMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_zoom50Menu_Text"), Command = _mainViewModel!.SetZoomCommand, CommandParameter = 0.5 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_zoom100Menu_Text"), Command = _mainViewModel!.SetZoomCommand, CommandParameter = 1.0 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_zoom150Menu_Text"), Command = _mainViewModel!.SetZoomCommand, CommandParameter = 1.5 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_zoom200Menu_Text"), Command = _mainViewModel!.SetZoomCommand, CommandParameter = 2.0 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_zoom300Menu_Text"), Command = _mainViewModel!.SetZoomCommand, CommandParameter = 3.0 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_zoom500Menu_Text"), Command = _mainViewModel!.SetZoomCommand, CommandParameter = 5.0 });
            zoomMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_zoom1000Menu_Text"), Command = _mainViewModel!.SetZoomCommand, CommandParameter = 10.0 });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_zoomMenu_Text"), Menu = zoomMenu });

            var intensityMenu = new NativeMenu();
            intensityMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_veryLowIntensityMenu_Text"), Command = _mainViewModel!.SetMapIntensityCommand, CommandParameter = 0.2 });
            intensityMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_lowIntensityMenu_Text"), Command = _mainViewModel!.SetMapIntensityCommand, CommandParameter = 0.4 });
            intensityMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_mediumIntensityMenu_Text"), Command = _mainViewModel!.SetMapIntensityCommand, CommandParameter = 0.6 });
            intensityMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_highIntensityMenu_Text"), Command = _mainViewModel!.SetMapIntensityCommand, CommandParameter = 0.8 });
            intensityMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_fullIntensityMenu_Text"), Command = _mainViewModel!.SetMapIntensityCommand, CommandParameter = 1.0 });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_mapIntensityMenu_Text"), Menu = intensityMenu });

            var qualityMenu = new NativeMenu();
            qualityMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_normalQualityMenu_Text"), Command = _mainViewModel!.SetNormalQualityCommand });
            qualityMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_highQualityMenu_Text"), Command = _mainViewModel!.SetHighQualityCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_mapQualityMenu_Text"), Menu = qualityMenu });

            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_showPrintAreaMenu_Text"), Command = _mainViewModel!.ToggleShowPrintAreaCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_showPopupsMenu_Text"), Command = _mainViewModel!.ToggleShowPopupsCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_allControlsMenu_Text"), Command = _mainViewModel!.ToggleAllControlsCommand, Gesture = new KeyGesture(Key.F4) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_otherCoursesMenu_Text"), Command = _mainViewModel!.ShowOtherCoursesCommand, Gesture = new KeyGesture(Key.F5) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_clearOtherCoursesMenu_Text"), Command = _mainViewModel!.ClearOtherCoursesCommand, Gesture = new KeyGesture(Key.F5, KeyModifiers.Shift) });
            return new NativeMenuItem { Header = L("MainFrame_viewMenu_Text"), Menu = menu };
        }

        private NativeMenuItem BuildAddMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addStartMenu_Text"), Command = _mainViewModel!.AddStartCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addControlMenu_Text"), Command = _mainViewModel!.AddControlCommand, Gesture = new KeyGesture(Key.A, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addFinishMenu_Text"), Command = _mainViewModel!.AddFinishCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addDescriptionsMenu_Text"), Command = _mainViewModel!.AddDescriptionsCommand });

            var mapExchangeMenu = new NativeMenu();
            mapExchangeMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_mapFlipMenuItem_Text"), Command = _mainViewModel!.AddMapFlipControlCommand });
            mapExchangeMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_mapExchangeControlMenuItem_Text"), Command = _mainViewModel!.AddMapExchangeControlCommand });
            mapExchangeMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_mapExchangeSeparateMenuItem_Text"), Command = _mainViewModel!.AddMapExchangeSeparateCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addMapExchangeMenu_Text"), Menu = mapExchangeMenu });

            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addVariationMenu_Text"), Command = _mainViewModel!.AddVariationCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addTextLineMenu_Text"), Command = _mainViewModel!.AddTextLineCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addMapIssueMenu_Text"), Command = _mainViewModel!.AddMapIssueCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addMandatoryCrossingMenu_Text"), Command = _mainViewModel!.AddMandatoryCrossingCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addOptCrossingMenu_Text"), Command = _mainViewModel!.AddOptCrossingCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addOutOfBoundsMenu_Text"), Command = _mainViewModel!.AddOutOfBoundsCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addDangerousMenu_Text"), Command = _mainViewModel!.AddDangerousCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addConstructionMenu_Text"), Command = _mainViewModel!.AddConstructionCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addWaterMenu_Text"), Command = _mainViewModel!.AddWaterCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addFirstAidMenu_Text"), Command = _mainViewModel!.AddFirstAidCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addForbiddenMenu_Text"), Command = _mainViewModel!.AddForbiddenCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addBoundaryMenu_Text"), Command = _mainViewModel!.AddBoundaryCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addRegMarkMenu_Text"), Command = _mainViewModel!.AddRegMarkCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_whiteOutMenu_Text"), Command = _mainViewModel!.AddWhiteOutCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addTextMenu_Text"), Command = _mainViewModel!.AddTextCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addImageMenu_Text"), Command = _mainViewModel!.AddImageCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addLineMenu_Text"), Command = _mainViewModel!.AddLineCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addRectangleMenu_Text"), Command = _mainViewModel!.AddRectangleCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addEllipseMenu_Text"), Command = _mainViewModel!.AddEllipseCommand });
            return new NativeMenuItem { Header = L("MainFrame_addMenu_Text"), Menu = menu };
        }

        private NativeMenuItem BuildEventMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_changeMapFileMenu_Text"), Command = _mainViewModel!.ChangeMapFileCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_changeCodesMenu_Text"), Command = _mainViewModel!.ChangeCodesCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_autoNumberingMenu_Text"), Command = _mainViewModel!.AutoNumberingCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_removeUnusedControlsMenu_Text"), Command = _mainViewModel!.RemoveUnusedControlsCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_moveAllControlsMenu_Text"), Command = _mainViewModel!.MoveAllControlsCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_punchPatternsMenu_Text"), Command = _mainViewModel!.PunchPatternsCommand });
            menu.Items.Add(new NativeMenuItemSeparator());

            var iofMenu = new NativeMenu();
            iofMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_descriptionStd2004Menu_Text"), Command = _mainViewModel!.SetDescriptionStd2004Command });
            iofMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_descriptionStd2018Menu_Text"), Command = _mainViewModel!.SetDescriptionStd2018Command });
            iofMenu.Items.Add(new NativeMenuItemSeparator());
            iofMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_mapStd2000Menu_Text"), Command = _mainViewModel!.SetMapStd2000Command });
            iofMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_mapStd2017Menu_Text"), Command = _mainViewModel!.SetMapStd2017Command });
            iofMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_mapStdSpr2019Menu_Text"), Command = _mainViewModel!.SetMapStdSpr2019Command });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_iOFStandardsToolStripMenuItem_Text"), Menu = iofMenu });

            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_customizeDescriptionsMenu_Text"), Command = _mainViewModel!.CustomizeDescriptionsCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_customizeCourseAppearanceMenu_Text"), Command = _mainViewModel!.CustomizeCourseAppearanceCommand });
            return new NativeMenuItem { Header = L("MainFrame_eventMenu_Text"), Menu = menu };
        }

        private NativeMenuItem BuildCourseMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addCourseMenu_Text"), Command = _mainViewModel!.ShowAddCourseDialogCommand, Gesture = new KeyGesture(Key.N, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_deleteCourseMenu_Text"), Command = _mainViewModel!.DeleteCourseCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_duplicateCourseMenu_Text"), Command = _mainViewModel!.DuplicateCourseCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_propertiesMenu_Text"), Command = _mainViewModel!.ShowCoursePropertiesCommand, Gesture = new KeyGesture(Key.OemComma, KeyModifiers.Meta | KeyModifiers.Shift) });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_courseOrderMenu_Text"), Command = _mainViewModel!.ShowCourseOrderCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_courseLoadMenu_Text"), Command = _mainViewModel!.ShowCourseLoadCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_courseVariationReportMenu_Text"), Command = _mainViewModel!.ShowCourseVariationReportCommand });
            return new NativeMenuItem { Header = L("MainFrame_courseMenu_Text"), Menu = menu };
        }

        private NativeMenuItem BuildItemMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_deleteItemMenu_Text"), Command = _mainViewModel!.DeleteSelectionCommand, Gesture = new KeyGesture(Key.Back, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_deleteForkMenu_Text"), Command = _mainViewModel!.DeleteForkCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addBendMenu_Text"), Command = _mainViewModel!.AddBendCommand, Gesture = new KeyGesture(Key.B, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_removeBendMenu_Text"), Command = _mainViewModel!.RemoveBendCommand, Gesture = new KeyGesture(Key.B, KeyModifiers.Meta | KeyModifiers.Shift) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_addGapMenu_Text"), Command = _mainViewModel!.AddGapCommand, Gesture = new KeyGesture(Key.G, KeyModifiers.Meta) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_removeGapMenu_Text"), Command = _mainViewModel!.RemoveGapCommand, Gesture = new KeyGesture(Key.G, KeyModifiers.Meta | KeyModifiers.Shift) });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_changeTextMenu_Text"), Command = _mainViewModel!.ChangeTextCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_changeLineAppearanceMenu_Text"), Command = _mainViewModel!.ChangeLineAppearanceCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_rotateMenu_Text"), Command = _mainViewModel!.RotateCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_stretchMenu_Text"), Command = _mainViewModel!.StretchCommand });

            var flaggingMenu = new NativeMenu();
            flaggingMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_noFlaggingMenu_Text"), Command = _mainViewModel!.SetNoFlaggingCommand });
            flaggingMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_entireFlaggingMenu_Text"), Command = _mainViewModel!.SetEntireFlaggingCommand });
            flaggingMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_beginFlaggingMenu_Text"), Command = _mainViewModel!.SetBeginFlaggingCommand });
            flaggingMenu.Items.Add(new NativeMenuItem { Header = L("MainFrame_endFlaggingMenu_Text"), Command = _mainViewModel!.SetEndFlaggingCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_legFlaggingMenu_Text"), Menu = flaggingMenu });

            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_changeDisplayedCoursesMenu_Text"), Command = _mainViewModel!.ChangeDisplayedCoursesCommand });
            return new NativeMenuItem { Header = L("MainFrame_itemMenu_Text"), Menu = menu };
        }

        private NativeMenuItem BuildReportsMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_courseSummaryMenu_Text"), Command = _mainViewModel!.ShowCourseSummaryCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_eventAuditMenu_Text"), Command = _mainViewModel!.ShowEventAuditCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_legLengthsMenu_Text"), Command = _mainViewModel!.ShowLegLengthsCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_controlCrossrefMenu_Text"), Command = _mainViewModel!.ShowControlCrossrefCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_controlAndLegLoadMenu_Text"), Command = _mainViewModel!.ShowControlAndLegLoadCommand });
            return new NativeMenuItem { Header = L("MainFrame_reportMenu_Text"), Menu = menu };
        }

        private NativeMenuItem BuildHelpMenu()
        {
            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_helpContentsMenu_Text"), Command = _mainViewModel!.HelpContentsCommand, Gesture = new KeyGesture(Key.F1) });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_helpTranslatedMenu_Text"), Command = _mainViewModel!.HelpTranslatedCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_mainWebSiteToolMenu_Text"), Command = _mainViewModel!.OpenMainWebSiteCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_supportWebSiteMenu_Text"), Command = _mainViewModel!.OpenSupportWebSiteCommand });
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_donateWebSiteMenu_Text"), Command = _mainViewModel!.OpenDonateWebSiteCommand });
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(new NativeMenuItem { Header = L("MainFrame_aboutMenu_Text"), Command = _mainViewModel!.ShowAboutDialogCommand });
            return new NativeMenuItem { Header = L("MainFrame_helpMenu_Text"), Menu = menu };
        }

        // Updates the "Open Recent" menu with recent files
        private void UpdateRecentFilesMenu()
        {
            if (_mainViewModel == null || _openRecentMenu?.Menu == null)
                return;

            _openRecentMenu.Menu.Items.Clear();

            // If no recent files, show a disabled placeholder
            if (_mainViewModel.RecentFiles.Count == 0) {
                var placeholderItem = new NativeMenuItem { Header = L("AvMainFrame_noRecentFiles_Text"), IsEnabled = false };
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

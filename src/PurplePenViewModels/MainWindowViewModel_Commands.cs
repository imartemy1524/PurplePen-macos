// These are the implementations of commands for the menu and toolbar
// in the main windows.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PurplePen.ViewModels
{
    public partial class MainWindowViewModel
    {
        // Update the state of menu items and toolbar buttons, which are
        // typically observable properties.
        private void UpdateMenusToolbarButtons()
        {
            if (controller == null) { return; }

            // Update enabled status.
            CanAddBend = (controller.CanAddBend() == CommandStatus.Enabled);

            // Update checked status of Zoom.
            Zoom50Checked = UpdateZoomChecked(0.5F);
            Zoom100Checked = UpdateZoomChecked(1.0F);
            Zoom150Checked = UpdateZoomChecked(1.5F);
            Zoom200Checked = UpdateZoomChecked(2.0F);
            Zoom300Checked = UpdateZoomChecked(3.0F);
            Zoom500Checked = UpdateZoomChecked(5.0F);
            Zoom1000Checked = UpdateZoomChecked(10.0F);

            // Update checked status of Intensity.
            IntensityVeryLowChecked = UpdateIntensityChecked(0.2F);
            IntensityLowChecked = UpdateIntensityChecked(0.4F);
            IntensityMediumChecked = UpdateIntensityChecked(0.6F);
            IntensityHighChecked = UpdateIntensityChecked(0.8F);
            IntensityFullChecked = UpdateIntensityChecked(1.0F);

            // Update checked status of Quality.
            HighQualityMapDisplay = MapDisplay?.AntiAlias ?? true;

            // Update checked status of Show All Controls.
            ViewAllControlsChecked = controller.ShowAllControls;

        }

        // Determine if the give zoom label (e.g. "100%") should be checked based on the current zoom factor.
        bool UpdateZoomChecked(float zoomLabel)
        {
            return Math.Abs(MapZoomFactor/zoomLabel - 1.0F) < 0.05F;
        }

        // Determine if the give zoom label (e.g. "100%") should be checked based on the current zoom factor.
        bool UpdateIntensityChecked(float intensityLabel)
        {
            if (MapDisplay == null) { return false; }

            return Math.Abs(MapDisplay.MapIntensity / intensityLabel - 1.0F) < 0.01F;
        }

        #region File commands

        /// <summary>
        /// Executes the File/New Event command. Shows the New Event wizard.
        /// </summary>
        [RelayCommand]
        private async Task NewEvent()
        {
            // Try to close the current file. If that succeeds, then ask for a new file and try to open it.
            if (controller == null) { return; }

            bool closeSuccess = await controller.TryCloseFile();
            if (closeSuccess) {
                NewEventWizardViewModel wizard = new NewEventWizardViewModel();
                bool result = await Services.DialogService.ShowDialogAsync(wizard);
                if (result) {
                    bool success = await controller.NewEvent(wizard.CreateEventInfo);
                    if (!success) {
#if !PORTING
                        // This is bad news. The old file is gone, and we don't have a new file. Go back to initial screen is the best solution,
                        // I guess.
                        Application.Idle -= new EventHandler(Application_Idle);
                        this.Dispose();
                        new InitialScreen().Show();
#endif
                    }
                }
            }
        }

        /// <summary>
        /// Shows the Open File dialog filtered to Purple Pen files (.ppen),
        /// and opens the selected file.
        /// </summary>
        [RelayCommand]
        private async Task FileOpenPurplePenFile()
        {
            if (controller == null) return;

#if PORTING
            // Not all functionality ported from MainFrame.openMenu_Click.
#endif
            FileOpenSingleViewModel fileOpenVM = new FileOpenSingleViewModel {
                FileFilters = MiscText.OpenFileDialog_PurplePenFilter,
                InitialFileFilterIndex = 1
            };

            bool result = await Services.DialogService.ShowDialogAsync(fileOpenVM);

            if (result && fileOpenVM.SelectedFile != null) {
                string newFilename = fileOpenVM.SelectedFile;
                bool success = await controller.LoadNewFile(newFilename);
            }
        }

        /// <summary>
        /// Executes the File/Save command.
        /// </summary>
        [RelayCommand]
        private void Save()
        {
            if (controller == null) { return; }

            controller.Save();
        }

        /// <summary>
        /// Executes the File/Save As command. Shows a Save File dialog.
        /// </summary>
        [RelayCommand]
        private async Task SaveAs()
        {
            if (controller == null) { return; }

            FileSaveViewModel fileSaveVm = new FileSaveViewModel {
                SuggestedFileName = Path.GetFileName(controller.FileName),
                InitialDirectory = Path.GetDirectoryName(controller.FileName),
                FileFilters = MiscText.OpenFileDialog_PurplePenFilter,
                DefaultExtension = "ppen"
            };

            bool result = await Services.DialogService.ShowDialogAsync(fileSaveVm);
            if (result && fileSaveVm.SelectedFile != null) {
                string newFileName = fileSaveVm.SelectedFile;
                controller.SaveAs(newFileName);
            }
        }

        /// <summary>
        /// Executes the File/Exit command. Closes the application.
        /// </summary>
        [RelayCommand]
        private async Task Exit()
        {
            if (controller != null && !await controller.TryCloseFile()) {
                return;
            }

            Environment.Exit(0);
        }

        #endregion // File commands

        #region Edit commands

        /// <summary>
        /// Executes the Edit/Cancel command. Cancels the current mode or clears selection.
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            if (controller == null) { return; }

            // Clear selection and cancel current mode use the same menu item.
            if (controller.CanCancelMode()) {
                controller.CancelMode();
            }
            else {
                controller.ClearSelection();
            }
        }

        /// <summary>
        /// Executes the Edit/Undo command.
        /// </summary>
        [RelayCommand]
        private void Undo()
        {
            if (controller == null) { return; }

            UndoStatus status = controller.GetUndoStatus();

            if (status.CanUndo)
                controller.Undo();
        }

        /// <summary>
        /// Executes the Edit/Redo command.
        /// </summary>
        [RelayCommand]
        private void Redo()
        {
            if (controller == null) { return; }

            UndoStatus status = controller.GetUndoStatus();

            if (status.CanRedo)
                controller.Redo();
        }

        /// <summary>
        /// Executes the Edit/Delete command. Deletes the current selection.
        /// </summary>
        [RelayCommand]
        private async Task DeleteSelection()
        {
            if (controller == null) { return; }

            await controller.DeleteSelection();
        }

        /// <summary>
        /// Executes the Edit/Delete Fork command.
        /// </summary>
        [RelayCommand]
        private async Task DeleteFork()
        {
#if !PORTING
            await controller.DeleteFork();
#endif
        }

        #endregion // Edit commands

        #region View commands

        /// <summary>
        /// Executes the View/Entire Course command. Zooms to show the entire course.
        /// </summary>
        [RelayCommand]
        private void ViewEntireCourse()
        {
#if !PORTING
            // Show the entire course.
            RectangleF courseBounds = controller.GetCourseBounds();
            ShowRectangle(courseBounds);
#endif
        }

        /// <summary>
        /// Executes the View/Entire Map command. Zooms to show the entire map.
        /// </summary>
        [RelayCommand]
        private void ViewEntireMap()
        {
#if !PORTING
            // Show the entire map.
            RectangleF mapBounds = mapDisplay.MapBounds;
            ShowRectangle(mapBounds);
#endif
        }

        /// <summary>
        /// Sets the zoom factor. Called from zoom menu items via CommandParameter.
        /// </summary>
        [RelayCommand]
        private void SetZoom(double zoomFactor)
        {
            MapZoomFactor = (float)zoomFactor;
        }

        // Bindable properties to indicate if a zoom level menu item should be checked.
        [ObservableProperty] private bool zoom50Checked;
        [ObservableProperty] private bool zoom100Checked;
        [ObservableProperty] private bool zoom150Checked;
        [ObservableProperty] private bool zoom200Checked;
        [ObservableProperty] private bool zoom300Checked;
        [ObservableProperty] private bool zoom500Checked;
        [ObservableProperty] private bool zoom1000Checked;


        /// <summary>
        /// Sets the map intensity. Called from intensity menu items via CommandParameter.
        /// </summary>
        [RelayCommand]
        private void SetMapIntensity(double intensity)
        {
            if (MapDisplay == null) { return; }

            MapDisplay.MapIntensity = (float)intensity;
            UserSettings.Current.MapIntensity = MapDisplay.MapIntensity;
            UserSettings.Current.Save();
        }

        [ObservableProperty] private bool intensityVeryLowChecked;
        [ObservableProperty] private bool intensityLowChecked;
        [ObservableProperty] private bool intensityMediumChecked;
        [ObservableProperty] private bool intensityHighChecked;
        [ObservableProperty] private bool intensityFullChecked;


        /// <summary>
        /// Toggles display of popup information.
        /// </summary>
        [RelayCommand]
        private void ToggleShowPopups()
        {
#if !PORTING
            showToolTips = !showToolTips;
            UserSettings.Current.ShowPopupInfo = showToolTips;
            UserSettings.Current.Save();
#endif
        }

        /// <summary>
        /// Toggles display of the print area.
        /// </summary>
        [RelayCommand]
        private void ToggleShowPrintArea()
        {
#if !PORTING
            UserSettings.Current.ShowPrintArea = !UserSettings.Current.ShowPrintArea;
            UserSettings.Current.Save();
            controller.ForceChangeUpdate(true);
#endif
        }

        /// <summary>
        /// Sets map rendering to high quality (anti-aliased).
        /// </summary>
        [RelayCommand]
        private void SetHighQuality()
        {
            SetQuality(true);
        }

        /// <summary>
        /// Sets map rendering to normal quality.
        /// </summary>
        [RelayCommand]
        private void SetNormalQuality()
        {
            SetQuality(false);
        }

        private void SetQuality(bool highQuality)
        {
            if (MapDisplay == null) { return; }

            MapDisplay.AntiAlias = highQuality;
            UserSettings.Current.MapHighQuality = highQuality;
            UserSettings.Current.Save();
        }


        [ObservableProperty]
        bool highQualityMapDisplay;

        /// <summary>
        /// Toggles the "show all controls" view mode.
        /// </summary>
        [RelayCommand]
        private void ToggleAllControls()
        {
            if (controller == null) { return; }

            controller.ShowAllControls = !controller.ShowAllControls;
            UserSettings.Current.ViewAllControls = controller.ShowAllControls;
            UserSettings.Current.Save();
        }

        [ObservableProperty]
        bool viewAllControlsChecked;

        /// <summary>
        /// Shows the View Additional Courses dialog.
        /// </summary>
        [RelayCommand]
        private void ShowOtherCourses()
        {
#if !PORTING
            ViewAdditionalCourses dialog = new ViewAdditionalCourses(controller.CurrentTabName, controller.CurrentCourseId);
            dialog.EventDB = controller.GetEventDB();
            dialog.DisplayedCourses = controller.ExtraCourseDisplay;
            if (dialog.ShowDialog() == DialogResult.OK) {
                controller.ExtraCourseDisplay = dialog.DisplayedCourses;
            }
#endif
        }

        /// <summary>
        /// Clears the extra course display.
        /// </summary>
        [RelayCommand]
        private void ClearOtherCourses()
        {
#if !PORTING
            controller.ClearExtraCourseDisplay();
#endif
        }

        #endregion // View commands

        #region Add control commands

        /// <summary>
        /// Executes the Add/Control command. Begins adding a normal control.
        /// </summary>
        [RelayCommand]
        private void AddControl()
        {
            if (controller == null) { return; }

            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.None);
        }

        /// <summary>
        /// Executes the Add/Start command. Begins adding a start control.
        /// </summary>
        [RelayCommand]
        private void AddStart()
        {
            if (controller == null) { return; }

            controller.BeginAddControlMode(ControlPointKind.Start, MapExchangeType.None);
        }

        /// <summary>
        /// Executes the Add/Finish command. Begins adding a finish control.
        /// </summary>
        [RelayCommand]
        private void AddFinish()
        {
            if (controller == null) { return; }

            controller.BeginAddControlMode(ControlPointKind.Finish, MapExchangeType.None);
        }

        /// <summary>
        /// Executes the Add/Map Exchange at Control command.
        /// </summary>
        [RelayCommand]
        private void AddMapExchangeControl()
        {
            if (controller == null) { return; }

            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.Exchange);
        }

        /// <summary>
        /// Executes the Add/Map Flip at Control command.
        /// </summary>
        [RelayCommand]
        private void AddMapFlipControl()
        {
            if (controller == null) { return; }

            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.MapFlip);
        }

        /// <summary>
        /// Executes the Add/Map Exchange (Separate) command.
        /// </summary>
        [RelayCommand]
        private void AddMapExchangeSeparate()
        {
            if (controller == null) { return; }

            controller.BeginAddControlMode(ControlPointKind.MapExchange, MapExchangeType.None);
        }

        /// <summary>
        /// Executes the Add/Descriptions command. Begins adding a description block.
        /// </summary>
        [RelayCommand]
        private void AddDescriptions()
        {
            if (controller == null) { return; }

            controller.BeginAddDescriptionMode();
        }

        /// <summary>
        /// Executes the Add/Variation command. Shows the Add Fork dialog.
        /// </summary>
        [RelayCommand]
        private async Task AddVariation()
        {
            if (controller == null) { return; }

            string reason;
            if (controller.CanAddVariation(out reason) != CommandStatus.Enabled) {
                await ErrorMessage(reason);
                return;
            }

            AddForkDialogViewModel viewModel = new AddForkDialogViewModel();
            bool result = await Services.DialogService.ShowDialogAsync(viewModel);
            if (result) {
                await controller.AddVariation(viewModel.Loop, viewModel.BranchCount);
            }
        }

        /// <summary>
        /// Executes the Add/Text Line command. Shows the Add Text Line dialog.
        /// </summary>
        [RelayCommand]
        private void AddTextLine()
        {
#if !PORTING
            string defaultText;
            DescriptionLine.TextLineKind defaultLineKind;
            bool enableThisCourse;
            string objectName;

            if (controller.CanAddTextLine(out defaultText, out defaultLineKind, out objectName, out enableThisCourse)) {
                // Initialize dialog.
                AddTextLine dialog = new AddTextLine(objectName, enableThisCourse);
                dialog.TextLine = defaultText;
                dialog.TextLineKind = defaultLineKind;

                // Show the dialog.
                DialogResult result = dialog.ShowDialog(this);

                // Apply changes.
                if (result == DialogResult.OK) {
                    controller.AddTextLine(dialog.TextLine, dialog.TextLineKind);
                }

                dialog.Dispose();
            }
#endif
        }

        #endregion // Add control commands

        #region Add special item commands

        /// <summary>
        /// Executes the Add/Map Issue command. Shows the Map Issue Choice dialog.
        /// </summary>
        [RelayCommand]
        private void AddMapIssue()
        {
#if !PORTING
            MapIssueChoiceDialog dialog = new MapIssueChoiceDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK) {
                controller.BeginAddMapIssuePointMode(dialog.MapIssueKind);
            }
            dialog.Dispose();
#endif
        }

        /// <summary>
        /// Executes the Add/Mandatory Crossing command.
        /// </summary>
        [RelayCommand]
        private void AddMandatoryCrossing()
        {
            if (controller == null) { return; }

            controller.BeginAddControlMode(ControlPointKind.CrossingPoint, MapExchangeType.None);
        }

        /// <summary>
        /// Executes the Add/Out of Bounds command.
        /// </summary>
        [RelayCommand]
        private void AddOutOfBounds()
        {
            if (controller == null) { return; }

            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.OOB, true);
        }

        /// <summary>
        /// Executes the Add/Dangerous command.
        /// </summary>
        [RelayCommand]
        private void AddDangerous()
        {
            if (controller == null) { return; }

            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.Dangerous, true);
        }

        /// <summary>
        /// Executes the Add/Construction command.
        /// </summary>
        [RelayCommand]
        private void AddConstruction()
        {
            if (controller == null) { return; }

            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.Construction, true);
        }

        /// <summary>
        /// Executes the Add/Boundary command.
        /// </summary>
        [RelayCommand]
        private void AddBoundary()
        {
            if (controller == null) { return; }

            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.Boundary, false);
        }

        /// <summary>
        /// Executes the Add/Optional Crossing command.
        /// </summary>
        [RelayCommand]
        private void AddOptCrossing()
        {
            if (controller == null) { return; }

            controller.BeginAddPointSpecialMode(SpecialKind.OptCrossing);
        }

        /// <summary>
        /// Executes the Add/Water command.
        /// </summary>
        [RelayCommand]
        private void AddWater()
        {
            if (controller == null) { return; }

            controller.BeginAddPointSpecialMode(SpecialKind.Water);
        }

        /// <summary>
        /// Executes the Add/First Aid command.
        /// </summary>
        [RelayCommand]
        private void AddFirstAid()
        {
            if (controller == null) { return; }

            controller.BeginAddPointSpecialMode(SpecialKind.FirstAid);
        }

        /// <summary>
        /// Executes the Add/Forbidden Route command.
        /// </summary>
        [RelayCommand]
        private void AddForbidden()
        {
            if (controller == null) { return; }

            controller.BeginAddPointSpecialMode(SpecialKind.Forbidden);
        }

        /// <summary>
        /// Executes the Add/Registration Mark command.
        /// </summary>
        [RelayCommand]
        private void AddRegMark()
        {
            if (controller == null) { return; }

            controller.BeginAddPointSpecialMode(SpecialKind.RegMark);
        }

        /// <summary>
        /// Executes the Add/White Out command.
        /// </summary>
        [RelayCommand]
        private void AddWhiteOut()
        {
            if (controller == null) { return; }

            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.WhiteOut, true);
        }

        /// <summary>
        /// Executes the Add/Text command. Shows the Change Text dialog for adding text.
        /// </summary>
        [RelayCommand]
        private void AddText()
        {
#if !PORTING
            short colorOcadId;
            float c, m, y, k;
            bool purpleOverprint;
            string fontName;
            bool fontBold, fontItalic;
            float fontHeight;
            bool fontAutoSize;
            SpecialColor fontColor;

            FindPurple.GetPurpleColor(mapDisplay, controller.GetCourseAppearance(), out colorOcadId, out c, out m, out y, out k, out purpleOverprint);

            ChangeText dialog = new ChangeText(MiscText.AddTextSpecialTitle, MiscText.AddTextSpecialExplanation, true,
                                               CmykColor.FromCmyk(c, m, y, k), controller.ExpandText);
            dialog.HelpTopic = "EditAddText.htm";

            controller.GetAddTextDefaultProperties(out fontName, out fontBold, out fontItalic, out fontColor, out fontHeight, out fontAutoSize);
            dialog.FontName = fontName;
            dialog.FontBold = fontBold;
            dialog.FontItalic = fontItalic;
            dialog.FontColor = fontColor;
            dialog.FontSize = fontHeight;
            dialog.FontSizeAutomatic = fontAutoSize;

            if (dialog.ShowDialog(this) == DialogResult.OK) {
                controller.BeginAddTextSpecialMode(dialog.UserText, dialog.FontName, dialog.FontBold, dialog.FontItalic, dialog.FontColor, dialog.FontSizeAutomatic ? -1 : dialog.FontSize);
            }

            dialog.Dispose();
#endif
        }

        /// <summary>
        /// Executes the Add/Image command. Shows an Open File dialog for image selection.
        /// </summary>
        [RelayCommand]
        private void AddImage()
        {
#if !PORTING
            openImageDialog.FileName = null;
            DialogResult result = openImageDialog.ShowDialog();

            if (result == DialogResult.OK) {
                string fileName = openImageDialog.FileName;
                controller.BeginAddImageSpecialMode(fileName);
            }
#endif
        }

        /// <summary>
        /// Executes the Add/Line command. Shows the Line Properties dialog.
        /// </summary>
        [RelayCommand]
        private void AddLine()
        {
#if !PORTING
            // Set the course appearance into the dialog
            CourseAppearance appearance = controller.GetCourseAppearance();

            // Get the correct default purple color to use.
            float c, m, y, k;
            bool purpleOverprint;
            short ocadId;
            FindPurple.GetPurpleColor(mapDisplay, appearance, out ocadId, out c, out m, out y, out k, out purpleOverprint);

            LinePropertiesDialog linePropertiesDialog = new LinePropertiesDialog(MiscText.AddLineTitle, MiscText.AddLineExplanation, "EditAddLine.htm", CmykColor.FromCmyk(c, m, y, k), appearance);

            // Get the defaults for a new line.
            SpecialColor color;
            LineKind lineKind;
            float lineWidth, gapSize, dashSize, cornerRadius;
            controller.GetLineSpecialProperties(SpecialKind.Line, false, out color, out lineKind, out lineWidth, out gapSize, out dashSize, out cornerRadius);
            linePropertiesDialog.ShowRadius = false;
            linePropertiesDialog.ShowLineKind = true;
            linePropertiesDialog.Color = color;
            linePropertiesDialog.LineKind = lineKind;
            linePropertiesDialog.LineWidth = lineWidth;
            linePropertiesDialog.GapSize = gapSize;
            linePropertiesDialog.DashSize = dashSize;

            DialogResult result = linePropertiesDialog.ShowDialog();

            if (result == DialogResult.OK) {
                controller.BeginAddLineSpecialMode(linePropertiesDialog.Color, linePropertiesDialog.LineKind, linePropertiesDialog.LineWidth, linePropertiesDialog.GapSize, linePropertiesDialog.DashSize);
            }

            linePropertiesDialog.Dispose();
#endif
        }

        /// <summary>
        /// Executes the Add/Rectangle command. Shows the Line Properties dialog.
        /// </summary>
        [RelayCommand]
        private void AddRectangle()
        {
#if !PORTING
            // Set the course appearance into the dialog
            CourseAppearance appearance = controller.GetCourseAppearance();

            // Get the correct default purple color to use.
            float c, m, y, k;
            bool purpleOverprint;
            short ocadId;
            FindPurple.GetPurpleColor(mapDisplay, appearance, out ocadId, out c, out m, out y, out k, out purpleOverprint);

            LinePropertiesDialog linePropertiesDialog = new LinePropertiesDialog(MiscText.AddRectangleTitle, MiscText.AddRectangleExplanation, "EditAddRectangle.htm", CmykColor.FromCmyk(c, m, y, k), appearance);

            // Get the defaults for a new line.
            SpecialColor color;
            LineKind lineKind;
            float lineWidth, gapSize, dashSize, cornerRadius;
            controller.GetLineSpecialProperties(SpecialKind.Rectangle, false, out color, out lineKind, out lineWidth, out gapSize, out dashSize, out cornerRadius);
            linePropertiesDialog.ShowRadius = true;
            linePropertiesDialog.ShowLineKind = false;
            linePropertiesDialog.Color = color;
            linePropertiesDialog.LineKind = LineKind.Single;
            linePropertiesDialog.LineWidth = lineWidth;
            linePropertiesDialog.GapSize = gapSize;
            linePropertiesDialog.DashSize = dashSize;
            linePropertiesDialog.CornerRadius = cornerRadius;

            DialogResult result = linePropertiesDialog.ShowDialog();

            if (result == DialogResult.OK) {
                controller.BeginAddRectangleSpecialMode(false, linePropertiesDialog.Color, linePropertiesDialog.LineKind, linePropertiesDialog.LineWidth, linePropertiesDialog.GapSize, linePropertiesDialog.DashSize, linePropertiesDialog.CornerRadius);
            }

            linePropertiesDialog.Dispose();
#endif
        }

        /// <summary>
        /// Executes the Add/Ellipse command. Shows the Line Properties dialog.
        /// </summary>
        [RelayCommand]
        private void AddEllipse()
        {
#if !PORTING
            // Set the course appearance into the dialog
            CourseAppearance appearance = controller.GetCourseAppearance();

            // Get the correct default purple color to use.
            float c, m, y, k;
            bool purpleOverprint;
            short ocadId;
            FindPurple.GetPurpleColor(mapDisplay, appearance, out ocadId, out c, out m, out y, out k, out purpleOverprint);

            LinePropertiesDialog linePropertiesDialog = new LinePropertiesDialog(MiscText.AddEllipseTitle, MiscText.AddEllipseExplanation, "EditAddEllipse.htm", CmykColor.FromCmyk(c, m, y, k), appearance);

            // Get the defaults for a new line.
            SpecialColor color;
            LineKind lineKind;
            float lineWidth, gapSize, dashSize, cornerRadius;
            controller.GetLineSpecialProperties(SpecialKind.Ellipse, false, out color, out lineKind, out lineWidth, out gapSize, out dashSize, out cornerRadius);
            linePropertiesDialog.ShowRadius = false;
            linePropertiesDialog.ShowLineKind = true;
            linePropertiesDialog.Color = color;
            linePropertiesDialog.LineKind = LineKind.Single;
            linePropertiesDialog.LineWidth = lineWidth;
            linePropertiesDialog.GapSize = gapSize;
            linePropertiesDialog.DashSize = dashSize;
            linePropertiesDialog.CornerRadius = cornerRadius;

            DialogResult result = linePropertiesDialog.ShowDialog();

            if (result == DialogResult.OK) {
                controller.BeginAddRectangleSpecialMode(true, linePropertiesDialog.Color, linePropertiesDialog.LineKind, linePropertiesDialog.LineWidth, linePropertiesDialog.GapSize, linePropertiesDialog.DashSize, 0);
            }

            linePropertiesDialog.Dispose();
#endif
        }

        #endregion // Add special item commands

        #region Item modification commands

        /// <summary>
        /// Executes the Item/Add Bend command.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanAddBend))]
        private void AddBend()
        {
            if (controller == null) { return; }
            controller.BeginAddBend();
        }

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(AddBendCommand))]
        private bool canAddBend;


        /// <summary>
        /// Executes the Item/Remove Bend command.
        /// </summary>
        [RelayCommand]
        private void RemoveBend()
        {
            if (controller == null) { return; }

            controller.BeginRemoveBend();
        }

        /// <summary>
        /// Executes the Item/Add Gap command.
        /// </summary>
        [RelayCommand]
        private void AddGap()
        {
            if (controller == null) { return; }

            controller.BeginAddGap();
        }

        /// <summary>
        /// Executes the Item/Remove Gap command.
        /// </summary>
        [RelayCommand]
        private void RemoveGap()
        {
            if (controller == null) { return; }

            controller.BeginRemoveGap();
        }

        /// <summary>
        /// Executes the Item/Rotate command.
        /// </summary>
        [RelayCommand]
        private void Rotate()
        {
            if (controller == null) { return; }

            controller.BeginRotate();
        }

        /// <summary>
        /// Executes the Item/Stretch command.
        /// </summary>
        [RelayCommand]
        private void Stretch()
        {
            if (controller == null) { return; }

            controller.BeginStretch();
        }

        /// <summary>
        /// Executes the Item/Change Text command. Shows the Change Text dialog.
        /// </summary>
        [RelayCommand]
        private void ChangeText()
        {
#if !PORTING
            if (controller.CanChangeText() == CommandStatus.Enabled) {
                short colorOcadId;
                float c, m, y, k;
                bool purpleOverprint;
                string fontName;
                bool fontBold, fontItalic;
                float fontHeight;
                SpecialColor fontColor;
                FindPurple.GetPurpleColor(mapDisplay, controller.GetCourseAppearance(), out colorOcadId, out c, out m, out y, out k, out purpleOverprint);

                string oldText = controller.GetChangableText();
                controller.GetChangableTextProperties(out fontName, out fontBold, out fontItalic, out fontColor, out fontHeight);
                ChangeText dialog = new ChangeText(MiscText.ChangeTextTitle, MiscText.ChangeTextSpecialExplanation, true,
                                                   CmykColor.FromCmyk(c, m, y, k), controller.ExpandText);
                dialog.HelpTopic = "ItemChangeText.htm";
                dialog.UserText = oldText;
                dialog.FontName = fontName;
                dialog.FontBold = fontBold;
                dialog.FontItalic = fontItalic;
                dialog.FontColor = fontColor;
                dialog.FontSize = (fontHeight < 0) ? 5 : fontHeight;
                dialog.FontSizeAutomatic = (fontHeight < 0);

                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    controller.ChangeText(dialog.UserText, dialog.FontName, dialog.FontBold, dialog.FontItalic, dialog.FontColor, dialog.FontSizeAutomatic ? -1 : dialog.FontSize);
                }

                dialog.Dispose();
            }
#endif
        }

        /// <summary>
        /// Executes the Item/Change Line Appearance command. Shows the Line Properties dialog.
        /// </summary>
        [RelayCommand]
        private void ChangeLineAppearance()
        {
#if !PORTING
            if (controller.CanChangeLineAppearance() == CommandStatus.Enabled) {
                CourseAppearance appearance = controller.GetCourseAppearance();

                short colorOcadId;
                float c, m, y, k;
                bool purpleOverprint;
                FindPurple.GetPurpleColor(mapDisplay, appearance, out colorOcadId, out c, out m, out y, out k, out purpleOverprint);

                LinePropertiesDialog linePropertiesDialog = new LinePropertiesDialog(MiscText.ChangeLineAppearanceTitle, MiscText.ChangeLineAppearanceExplanation, "ItemChangeLineAppearance.htm", CmykColor.FromCmyk(c, m, y, k), appearance);

                // Get the defaults for a new line.
                SpecialColor color;
                LineKind lineKind;
                bool showRadius;
                float lineWidth, gapSize, dashSize, cornerRadius;
                controller.GetChangableLineProperties(out showRadius, out color, out lineKind, out lineWidth, out gapSize, out dashSize, out cornerRadius);
                linePropertiesDialog.ShowRadius = showRadius;
                linePropertiesDialog.ShowLineKind = !showRadius;
                linePropertiesDialog.Color = color;
                linePropertiesDialog.LineKind = lineKind;
                linePropertiesDialog.LineWidth = lineWidth;
                linePropertiesDialog.GapSize = gapSize;
                linePropertiesDialog.DashSize = dashSize;
                linePropertiesDialog.CornerRadius = cornerRadius;

                DialogResult result = linePropertiesDialog.ShowDialog();

                if (result == DialogResult.OK) {
                    controller.ChangeLineAppearance(linePropertiesDialog.Color, linePropertiesDialog.LineKind, linePropertiesDialog.LineWidth, linePropertiesDialog.GapSize, linePropertiesDialog.DashSize, linePropertiesDialog.CornerRadius);
                }

                linePropertiesDialog.Dispose();
            }
#endif
        }

        /// <summary>
        /// Executes the Item/Change Displayed Courses command.
        /// </summary>
        [RelayCommand]
        private void ChangeDisplayedCourses()
        {
#if !PORTING
            CourseDesignator[] displayedCourses;
            bool showAllControls;

            if (controller.CanChangeDisplayedCourses(out displayedCourses, out showAllControls) == CommandStatus.Enabled) {
                ChangeSpecialCourses changeCoursesDialog = new ChangeSpecialCourses();
                changeCoursesDialog.EventDB = controller.GetEventDB();
                changeCoursesDialog.ShowAllControls = showAllControls;
                changeCoursesDialog.DisplayedCourses = displayedCourses;

                DialogResult result = changeCoursesDialog.ShowDialog(this);
                if (result == DialogResult.OK) {
                    controller.ChangeDisplayedCourses(changeCoursesDialog.DisplayedCourses);
                }
            }
#endif
        }

        #endregion // Item modification commands

        #region Leg flagging commands

        /// <summary>
        /// Executes the Leg/No Flagging command.
        /// </summary>
        [RelayCommand]
        private void SetNoFlagging()
        {
            if (controller == null) { return; }

            controller.SetLegFlagging(FlaggingKind.None);
        }

        /// <summary>
        /// Executes the Leg/Entire Leg Flagging command.
        /// </summary>
        [RelayCommand]
        private void SetEntireFlagging()
        {
            if (controller == null) { return; }

            controller.SetLegFlagging(FlaggingKind.All);
        }

        /// <summary>
        /// Executes the Leg/Begin Flagging command.
        /// </summary>
        [RelayCommand]
        private void SetBeginFlagging()
        {
            if (controller == null) { return; }

            controller.SetLegFlagging(FlaggingKind.Begin);
        }

        /// <summary>
        /// Executes the Leg/End Flagging command.
        /// </summary>
        [RelayCommand]
        private void SetEndFlagging()
        {
            if (controller == null) { return; }

            controller.SetLegFlagging(FlaggingKind.End);
        }

        #endregion // Leg flagging commands

        #region Course commands

        /// <summary>
        /// Shows the Add Course dialog.
        /// </summary>
        [RelayCommand]
        private async Task ShowAddCourseDialog()
        {
            if (controller == null) return;

            AddCourseDialogViewModel vm = new AddCourseDialogViewModel();
            InitializeAddCourseDialogForNewCourse(vm);

            bool result = await Services.DialogService.ShowDialogAsync(vm);
            if (result) {
                controller.NewCourse(vm.CourseKind, vm.CourseName, vm.ControlLabelKind, vm.ScoreColumn,
                    vm.SecondaryTitlePipeDelimited ?? string.Empty, vm.PrintScale, vm.Climb, vm.Length, vm.DescKind,
                    vm.FirstControlOrdinal, vm.HideFromReports);
            }
        }

        /// <summary>
        /// Executes the Course/Delete Course command.
        /// </summary>
        [RelayCommand]
        private async Task DeleteCourse()
        {
            if (controller == null) return;

            if (controller.CanDeleteCurrentCourse()) {
                await controller.DeleteCurrentCourse();
            }
        }

        /// <summary>
        /// Executes the Course/Duplicate Course command. Shows the Add Course dialog
        /// pre-populated with current course properties.
        /// </summary>
        [RelayCommand]
        private async Task DuplicateCourse()
        {
            if (controller == null) { return; }

            if (controller.CanDuplicateCurrentCourse()) {
                AddCourseDialogViewModel vm = new AddCourseDialogViewModel();
                InitializeAddCourseDialogWithCurrentValues(vm);
                vm.DialogTitle = MiscText.DuplicateCourseTitle;
                vm.CourseName = "";
                vm.CanChangeCourseKind = false;

                bool result = await Services.DialogService.ShowDialogAsync(vm);
                if (result) {
                    controller.DuplicateCurrentCourse(vm.CourseName, vm.ControlLabelKind, vm.ScoreColumn,
                        vm.SecondaryTitlePipeDelimited ?? string.Empty, vm.PrintScale, vm.Climb, vm.Length, vm.DescKind,
                        vm.FirstControlOrdinal, vm.HideFromReports);
                }
            }
        }

        /// <summary>
        /// Executes the Course/Properties command. Shows the course properties dialog.
        /// </summary>
        [RelayCommand]
        private async Task ShowCourseProperties()
        {
            if (controller == null) { return; }

            if (controller.CanChangeCourseProperties()) {
                AddCourseDialogViewModel vm = new AddCourseDialogViewModel();
                InitializeAddCourseDialogWithCurrentValues(vm);
                vm.DialogTitle = MiscText.CoursePropertiesTitle;

                bool result = await Services.DialogService.ShowDialogAsync(vm);
                if (result) {
                    controller.ChangeCurrentCourseProperties(vm.CourseKind, vm.CourseName, vm.ControlLabelKind,
                        vm.ScoreColumn, vm.SecondaryTitlePipeDelimited ?? string.Empty, vm.PrintScale, vm.Climb, vm.Length,
                        vm.DescKind, vm.FirstControlOrdinal, vm.HideFromReports);
                }
            }
            else {
                AllControlsPropertiesDialogViewModel vm = new AllControlsPropertiesDialogViewModel();
                float printScale;
                DescriptionKind descKind;
                controller.GetAllControlsProperties(out printScale, out descKind);
                vm.InitializePrintScales(controller.MapScale);
                vm.PrintScale = printScale;
                vm.DescKind = descKind;

                bool result = await Services.DialogService.ShowDialogAsync(vm);
                if (result) {
                    controller.ChangeAllControlsProperties(vm.PrintScale, vm.DescKind);
                }
            }
        }

        /// <summary>
        /// Executes the Course/Course Load command. Shows the Course Load dialog.
        /// </summary>
        [RelayCommand]
        private async Task ShowCourseLoad()
        {
            if (controller == null) { return; }

            CourseLoadDialogViewModel vm = new CourseLoadDialogViewModel();
            vm.SetCourseLoads(controller.GetAllCourseLoads());

            bool result = await Services.DialogService.ShowDialogAsync(vm);
            if (result) {
                controller.SetAllCourseLoads(vm.GetCourseLoads());
            }
        }

        /// <summary>
        /// Executes the Course/Course Order command. Shows the Change Course Order dialog.
        /// </summary>
        [RelayCommand]
        private async Task ShowCourseOrder()
        {
            if (controller == null) { return; }

            ChangeCourseOrderDialogViewModel vm = new ChangeCourseOrderDialogViewModel();
            vm.SetCourseOrders(controller.GetAllCourseOrders());

            bool result = await Services.DialogService.ShowDialogAsync(vm);
            if (result) {
                controller.SetAllCourseOrders(vm.GetCourseOrders());
            }
        }

        /// <summary>
        /// Executes the Course/Course Variation Report command.
        /// </summary>
        [RelayCommand]
        private async Task ShowCourseVariationReport()
        {
            if (controller == null) { return; }

            RelaySettings? relaySettings = controller.GetRelayParameters();
            if (relaySettings == null) {
                return;
            }

            bool hideVariationsOnMap = controller.GetHideVariationsOnMap();
            TeamVariationsDialogViewModel vm = new TeamVariationsDialogViewModel();
            vm.Initialize(controller, relaySettings, hideVariationsOnMap, controller.GetDefaultVariationExportFileName());

            bool result = await Services.DialogService.ShowDialogAsync(vm);
            if (result && (relaySettings.firstTeamNumber != vm.FirstTeamNumber ||
                relaySettings.relayTeams != vm.NumberOfTeams ||
                relaySettings.relayLegs != vm.NumberOfLegs ||
                hideVariationsOnMap != vm.HideVariationsOnMap ||
                !object.Equals(relaySettings.relayBranchAssignments, vm.FixedBranchAssignments)))
            {
                controller.SetRelayParameters(vm.RelaySettings, vm.HideVariationsOnMap);
            }
        }

        /// <summary>
        /// Populates the Add Course dialog with values for a brand-new course.
        /// </summary>
        private void InitializeAddCourseDialogForNewCourse(AddCourseDialogViewModel vm)
        {
            Controller currentController = controller!;
            float printScale;
            DescriptionKind descKind;
            currentController.GetAllControlsProperties(out printScale, out descKind);

            vm.InitializePrintScales(currentController.MapScale);
            vm.PrintScale = printScale;
            vm.DescKind = descKind;
            vm.CanChangeCourseKind = true;
        }

        /// <summary>
        /// Populates the Add Course dialog with the properties of the current course.
        /// </summary>
        private void InitializeAddCourseDialogWithCurrentValues(AddCourseDialogViewModel vm)
        {
            Controller currentController = controller!;
            CourseKind courseKind;
            string courseName;
            string secondaryTitle;
            float printScale;
            float climb;
            float? length;
            DescriptionKind descKind;
            int firstControlOrdinal;
            ControlLabelKind labelKind;
            int scoreColumn;
            bool hideFromReports;
            currentController.GetCurrentCourseProperties(out courseKind, out courseName, out labelKind, out scoreColumn, out secondaryTitle,
                out printScale, out climb, out length, out descKind, out firstControlOrdinal, out hideFromReports);

            vm.InitializePrintScales(currentController.MapScale);
            vm.CourseKind = courseKind;
            vm.CourseName = courseName;
            vm.SecondaryTitlePipeDelimited = secondaryTitle;
            vm.PrintScale = printScale;
            vm.Climb = climb;
            vm.Length = length;
            vm.DescKind = descKind;
            vm.FirstControlOrdinal = firstControlOrdinal;
            vm.ControlLabelKind = labelKind;
            vm.ScoreColumn = scoreColumn;
            vm.HideFromReports = hideFromReports;
        }

        #endregion // Course commands

        #region Event/tools commands

        /// <summary>
        /// Executes the Event/Change Map File command. Shows the Change Map File dialog.
        /// </summary>
        [RelayCommand]
        private void ChangeMapFile()
        {
#if !PORTING
            // Initialize dialog.
            ChangeMapFile dialog = new ChangeMapFile();
            dialog.MapFile = controller.MapFileName;
            if (controller.MapType == MapType.Bitmap) {
                dialog.MapScale = controller.MapScale;   // Note: these must be set AFTER the MapFile property
                dialog.Dpi = controller.MapDpi;
            }
            else if (controller.MapType == MapType.PDF) {
                dialog.MapScale = controller.MapScale;
            }

            // Show the dialog.
            DialogResult result = dialog.ShowDialog(this);

            // Apply new map file.
            if (result == DialogResult.OK) {
                controller.ChangeMapFile(dialog.MapType, dialog.MapFile, dialog.MapScale, dialog.Dpi);
            }
#endif
        }

        /// <summary>
        /// Executes the Event/Change Codes command. Shows the Change All Codes dialog.
        /// </summary>
        [RelayCommand]
        private void ChangeCodes()
        {
#if !PORTING
            // Initialize the dialog with the current codes.
            ChangeAllCodes changeCodesDialog = new ChangeAllCodes();
            changeCodesDialog.SetEventDB(controller.GetEventDB());
            changeCodesDialog.Codes = controller.GetAllControlCodes();

            // Show the dialog to allow people to change the codes.
            DialogResult result = changeCodesDialog.ShowDialog(this);

            // Apply the changes.
            if (result == DialogResult.OK) {
                controller.SetAllControlCodes(changeCodesDialog.Codes);
            }

            changeCodesDialog.Dispose();
#endif
        }

        /// <summary>
        /// Executes the Event/Auto Numbering command. Shows the Auto Numbering dialog.
        /// </summary>
        [RelayCommand]
        private void AutoNumbering()
        {
#if !PORTING
            // Get initial values.
            int firstCode;
            bool disallowInvertibleCodes;

            controller.GetAutoNumbering(out firstCode, out disallowInvertibleCodes);

            // Initialize dialog.
            AutoNumbering autoNumberingDialog = new AutoNumbering();
            autoNumberingDialog.FirstCode = firstCode;
            autoNumberingDialog.DisallowInvertibleCodes = disallowInvertibleCodes;
            autoNumberingDialog.RenumberExisting = false;

            // Show the dialog.
            DialogResult result = autoNumberingDialog.ShowDialog(this);

            // Apply the changes.
            if (result == DialogResult.OK) {
                controller.AutoNumbering(autoNumberingDialog.FirstCode, autoNumberingDialog.DisallowInvertibleCodes, autoNumberingDialog.RenumberExisting);
            }

            autoNumberingDialog.Dispose();
#endif
        }

        /// <summary>
        /// Executes the Event/Remove Unused Controls command.
        /// </summary>
        [RelayCommand]
        private async Task RemoveUnusedControls()
        {
#if !PORTING
            List<KeyValuePair<Id<ControlPoint>,string>> unusedControls = controller.GetUnusedControls();

            if (unusedControls.Count == 0) {
                // No controls to delete. Tell the user.
                await InfoMessage(MiscText.NoUnusedControls);
            }
            else {
                // Put up the dialog and do it.
                UnusedControls dialog = new UnusedControls();
                dialog.SetControlsToDelete(controller.GetUnusedControls());

                if (dialog.ShowDialog() == DialogResult.OK) {
                    controller.RemoveControls(dialog.GetControlsToDelete());
                }

                dialog.Dispose();
            }
#endif
        }

        /// <summary>
        /// Executes the Event/Move All Controls command.
        /// </summary>
        [RelayCommand]
        private void MoveAllControls()
        {
#if !PORTING
            // Part 1: Determine which action we are doing.
            MoveAllControls moveAllControlsDialog = new MoveAllControls();
            if (moveAllControlsDialog.ShowDialog() == DialogResult.Cancel) {
                moveAllControlsDialog.Dispose();
                return;
            }

            MoveAllControlsAction action = moveAllControlsDialog.Action;
            moveAllControlsDialog.Dispose();

            // Part 2: Prompt use to move controls
            controller.BeginMoveAllControls();

            SelectLocationsForMove selectLocationsForMoveDialog = new SelectLocationsForMove(controller, action);
            Point location = this.Location;
            location.Offset(10, 130);
            selectLocationsForMoveDialog.Location = location;
            selectLocationsForMoveDialog.Show(this);

            // Dialog dismisses/disposes itself and invokes controller.
#endif
        }

        /// <summary>
        /// Executes the Event/Punch Patterns command. Shows the Punch Pattern dialog.
        /// </summary>
        [RelayCommand]
        private void PunchPatterns()
        {
#if !PORTING
            // Get all the punch patterns and the punch card layout.
            Dictionary<string, PunchPattern> allPatterns = controller.GetAllPunchPatterns();
            PunchcardFormat punchcardFormat = controller.GetPunchcardFormat();

            // Initialize the dialog.
            PunchPatternDialog dialog = new PunchPatternDialog();
            dialog.AllPunchPatterns = allPatterns;
            dialog.PunchcardFormat = punchcardFormat;

            // Show the dialog.
            DialogResult result = dialog.ShowDialog(this);

            // Apply the changes.
            if (result == DialogResult.OK) {
                if (!dialog.PunchcardFormat.Equals(punchcardFormat))
                    controller.SetPunchcardFormat(dialog.PunchcardFormat);
                controller.SetAllPunchPatterns(dialog.AllPunchPatterns);
            }

            dialog.Dispose();
#endif
        }

        /// <summary>
        /// Executes the Event/Customize Descriptions command. Shows the Custom Symbol Text dialog.
        /// </summary>
        [RelayCommand]
        private void CustomizeDescriptions()
        {
#if !PORTING
            Dictionary<string, List<SymbolText>> customSymbolText;
            Dictionary<string, bool> customSymbolKey;

            // Initialize the dialog
            CustomSymbolText dialog = new CustomSymbolText(symbolDB, false);
            controller.GetCustomSymbolText(out customSymbolText, out customSymbolKey);
            dialog.SetCustomSymbolDictionaries(customSymbolText, customSymbolKey);
            dialog.LangId = controller.GetDescriptionLanguage();

            // Show the dialog.
            DialogResult result = dialog.ShowDialog(this);

            // Apply the changes
            if (result == DialogResult.OK) {
                // dialog changes the dictionaries, so we don't need to retrieve them.
                controller.SetCustomSymbolText(customSymbolText, customSymbolKey, dialog.LangId);
                if (dialog.UseAsDefaultLanguage)
                    controller.DefaultDescriptionLanguage = dialog.LangId;
            }

            dialog.Dispose();
#endif
        }

        /// <summary>
        /// Executes the Event/Customize Course Appearance command.
        /// </summary>
        [RelayCommand]
        private void CustomizeCourseAppearance()
        {
#if !PORTING
            // Initialize the dialog
            CourseAppearanceDialog dialog = new CourseAppearanceDialog();

            // Get the correct default purple color to use.
            float c, m, y, k;
            bool purpleOverprint;
            short ocadId;
            FindPurple.GetPurpleColor(mapDisplay, null, out ocadId, out c, out m, out y, out k, out purpleOverprint);
            dialog.SetDefaultPurple(c, m, y, k);
            dialog.UsesOcadMap = (mapDisplay.MapType == MapType.OCAD);
            dialog.SetMapLayers(controller.GetUnderlyingMapColors());

            // Set the course appearance into the dialog
            CourseAppearance appearance = controller.GetCourseAppearance();
            if (dialog.UsesOcadMap && appearance.purpleColorBlend != PurpleColorBlend.UpperLowerPurple) {
                // Set the default lower purple layer anyway, so that it is chosen by default when the user changes the blend.
                appearance.mapLayerForLowerPurple = controller.GetDefaultLowerPurpleLayer();
            }
            dialog.CourseAppearance = appearance;

            // Show the dialog.
            if (dialog.ShowDialog(this) == DialogResult.OK) {
                controller.SetCourseAppearance(dialog.CourseAppearance);
            }

            dialog.Dispose();
#endif
        }

        #endregion // Event/tools commands

        #region IOF Standards commands

        /// <summary>
        /// Sets the description standard to 2004.
        /// </summary>
        [RelayCommand]
        private void SetDescriptionStd2004()
        {
            if (controller == null) { return; }

            controller.ChangeDescriptionStandard("2004");
        }

        /// <summary>
        /// Sets the description standard to 2018.
        /// </summary>
        [RelayCommand]
        private void SetDescriptionStd2018()
        {
            if (controller == null) { return; }

            controller.ChangeDescriptionStandard("2018");
        }

        /// <summary>
        /// Sets the map standard to 2000.
        /// </summary>
        [RelayCommand]
        private void SetMapStd2000()
        {
            if (controller == null) { return; }

            controller.ChangeMapStandard("2000");
        }

        /// <summary>
        /// Sets the map standard to 2017.
        /// </summary>
        [RelayCommand]
        private void SetMapStd2017()
        {
            if (controller == null) { return; }

            controller.ChangeMapStandard("2017");
        }

        /// <summary>
        /// Sets the map standard to Sprint 2019.
        /// </summary>
        [RelayCommand]
        private void SetMapStdSpr2019()
        {
            if (controller == null) { return; }

            controller.ChangeMapStandard("Spr2019");
        }

        #endregion // IOF Standards commands

        #region Print area commands

        /// <summary>
        /// Sets the print area for this part only.
        /// </summary>
        [RelayCommand]
        private async Task SetPrintAreaThisPart()
        {
            await SetPrintArea(PrintAreaKind.OnePart);
        }

        /// <summary>
        /// Sets the print area for this course only.
        /// </summary>
        [RelayCommand]
        private async Task SetPrintAreaThisCourse()
        {
            await SetPrintArea(PrintAreaKind.OneCourse);
        }

        /// <summary>
        /// Sets the print area for all courses.
        /// </summary>
        [RelayCommand]
        private async Task SetPrintAreaAllCourses()
        {
            await SetPrintArea(PrintAreaKind.AllCourses);
        }

        /// <summary>
        /// Shows the print area editor and applies or cancels the controller rectangle mode.
        /// </summary>
        /// <param name="printAreaKind">The print area scope to edit.</param>
        private async Task SetPrintArea(PrintAreaKind printAreaKind)
        {
            if (controller == null) { return; }

            SetPrintAreaDialogViewModel viewModel = new SetPrintAreaDialogViewModel();
            viewModel.Initialize(controller, printAreaKind);
            controller.BeginSetPrintArea(printAreaKind, NullDisposable.Instance);
            viewModel.SendCurrentSettingsToController();

            bool result = await Services.DialogService.ShowDialogAsync(viewModel);
            if (result) {
                viewModel.Apply();
            }
            else {
                viewModel.Cancel();
            }
        }

        #endregion // Print area commands

        #region Print and export commands

        /// <summary>
        /// Executes the File/Print Descriptions command.
        /// </summary>
        [RelayCommand]
        private async Task PrintDescriptions()
        {
            if (controller == null) { return; }

            DescriptionPrintSettings settings = new DescriptionPrintSettings {
                CourseIds = QueryEvent.SortedCourseIds(controller.GetEventDB(), true),
                AllCourses = true
            };

            string fileName = Path.Combine(Path.GetTempPath(), CurrentEventBaseName() + "-descriptions-" + Guid.NewGuid().ToString("N") + ".pdf");
            controller.CreateDescriptionsPdf(settings, GetDefaultPrintingPaperSizeWithMargins(), fileName);
            await OpenFileWithDefaultApplication(fileName);
        }

        /// <summary>
        /// Executes the File/Create Description PDF command.
        /// </summary>
        [RelayCommand]
        private async Task CreateDescriptionPdf()
        {
            if (controller == null) { return; }

            DescriptionPrintSettings settings = new DescriptionPrintSettings {
                CourseIds = QueryEvent.SortedCourseIds(controller.GetEventDB(), true),
                AllCourses = true
            };

            FileSaveViewModel savePdfDialog = new FileSaveViewModel {
                FileFilters = MiscText.PdfFilter,
                DefaultExtension = "pdf",
                InitialDirectory = Path.GetDirectoryName(controller.FileName),
                SuggestedFileName = Path.GetFileNameWithoutExtension(controller.FileName) + "-descriptions.pdf"
            };

            if (await Services.DialogService.ShowDialogAsync(savePdfDialog) && savePdfDialog.SelectedFile != null) {
                controller.CreateDescriptionsPdf(settings, GetDefaultPrintingPaperSizeWithMargins(), savePdfDialog.SelectedFile);
            }
        }

        /// <summary>
        /// Executes the File/Print Punch Cards command.
        /// </summary>
        [RelayCommand]
        private async Task PrintPunchCards()
        {
            if (controller == null) { return; }

            CorePunchPrintSettings settings = new CorePunchPrintSettings {
                CourseIds = QueryEvent.SortedCourseIds(controller.GetEventDB(), true),
                AllCourses = true,
                Count = 1
            };

            string fileName = Path.Combine(Path.GetTempPath(), CurrentEventBaseName() + "-punchcards-" + Guid.NewGuid().ToString("N") + ".pdf");
            controller.CreatePunchesPdf(settings, GetDefaultPrintingPaperSizeWithMargins(), fileName);
            await OpenFileWithDefaultApplication(fileName);
        }

        /// <summary>
        /// Executes the File/Create Punchcard PDF command.
        /// </summary>
        [RelayCommand]
        private async Task CreatePunchcardPdf()
        {
            if (controller == null) { return; }

            CorePunchPrintSettings settings = new CorePunchPrintSettings {
                CourseIds = QueryEvent.SortedCourseIds(controller.GetEventDB(), true),
                AllCourses = true,
                Count = 1
            };

            FileSaveViewModel savePdfDialog = new FileSaveViewModel {
                FileFilters = MiscText.PdfFilter,
                DefaultExtension = "pdf",
                InitialDirectory = Path.GetDirectoryName(controller.FileName),
                SuggestedFileName = Path.GetFileNameWithoutExtension(controller.FileName) + "-punchcards.pdf"
            };

            if (await Services.DialogService.ShowDialogAsync(savePdfDialog) && savePdfDialog.SelectedFile != null) {
                controller.CreatePunchesPdf(settings, GetDefaultPrintingPaperSizeWithMargins(), savePdfDialog.SelectedFile);
            }
        }

        /// <summary>
        /// Executes the File/Print Courses command.
        /// </summary>
        [RelayCommand]
        private async Task PrintCourses()
        {
            if (controller == null) { return; }

            string[] nonRenderableObjects = controller.NonrenderableObjects(false);
            if (nonRenderableObjects != null && nonRenderableObjects.Length > 0) {
                bool continueResult = await YesNoQuestion(
                    "The following objects cannot be rendered in a PDF and will be omitted:\n\n" +
                    string.Join("\n", nonRenderableObjects) + "\n\nContinue?",
                    false);
                if (!continueResult) {
                    return;
                }
            }

            string outputDirectory = Path.Combine(Path.GetTempPath(), "PurplePenPrint-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(outputDirectory);

            CoursePdfSettings settings = new CoursePdfSettings {
                CourseIds = QueryEvent.SortedCourseIds(controller.GetEventDB(), true),
                AllCourses = true,
                FileCreation = CoursePdfSettings.PdfFileCreation.SingleFile,
                ColorModel = ColorModel.CMYK,
                RenderControlDescriptions = true,
                ShowProgressDialog = false,
                fileDirectory = false,
                mapDirectory = false,
                outputDirectory = outputDirectory,
                filePrefix = "print"
            };

            if (controller.MapType == MapType.PDF) {
                settings.CropLargePrintArea = true;
            }

            controller.CreateCoursePdfs(settings);

            string[] createdFiles = Directory.GetFiles(outputDirectory, "*.pdf");
            if (createdFiles.Length == 0) {
                await ErrorMessage("No PDF was created for printing.");
                return;
            }

            await OpenFileWithDefaultApplication(createdFiles[0]);
        }

        /// <summary>
        /// Executes the File/Create Course PDF command.
        /// </summary>
        [RelayCommand]
        private async Task CreateCoursePdf()
        {
            if (controller == null) { return; }

            string[] nonRenderableObjects = controller.NonrenderableObjects(false);
            if (nonRenderableObjects != null && nonRenderableObjects.Length > 0) {
                bool continueResult = await YesNoQuestion(
                    "The following objects cannot be rendered in a PDF and will be omitted:\n\n" +
                    string.Join("\n", nonRenderableObjects) + "\n\nContinue?",
                    false);
                if (!continueResult) {
                    return;
                }
            }

            CoursePdfSettings settings = coursePdfSettingsPrevious != null ? coursePdfSettingsPrevious.Clone() : new CoursePdfSettings();
            if (coursePdfSettingsPrevious == null) {
                settings.fileDirectory = true;
                settings.mapDirectory = false;
                settings.outputDirectory = Path.GetDirectoryName(controller.FileName) ?? "";
            }
            settings.AllCourses = true;
            settings.CourseIds = QueryEvent.SortedCourseIds(controller.GetEventDB(), true);
            if (controller.MapType == MapType.PDF) {
                settings.CropLargePrintArea = true;
            }

            CreatePdfCoursesViewModel viewModel = new CreatePdfCoursesViewModel();
            viewModel.Initialize(controller.GetEventDB(), controller.AnyMultipart(), settings);
            if (controller.MapType == MapType.PDF) {
                viewModel.CanChangeCropping = false;
                viewModel.MultiPageIndex = 0;
            }

            bool result = await Services.DialogService.ShowDialogAsync(viewModel);
            if (!result) {
                return;
            }

            settings = viewModel.BuildSettings();
            List<string> overwritingFiles = controller.OverwritingPdfFiles(settings);
            if (overwritingFiles.Count > 0) {
                bool continueResult = await YesNoQuestion(
                    "The following files already exist and will be overwritten:\n\n" + string.Join("\n", overwritingFiles) + "\n\nContinue?",
                    false);
                if (!continueResult) {
                    return;
                }
            }

            coursePdfSettingsPrevious = settings;
            controller.CreateCoursePdfs(settings);
        }

        /// <summary>
        /// Gets the default page size and margins for PDF creation based on the current culture.
        /// </summary>
        private PrintingPaperSizeWithMargins GetDefaultPrintingPaperSizeWithMargins()
        {
            bool metric = Util.IsCurrentCultureMetric();
            PrintingPaperSize paperSize = PrintingStandards.StandardPaperSizes[
                metric ? PrintingStandards.DefaultMetricPaperSizeindex : PrintingStandards.DefaultEnglighPaperSizeIndex];
            PrintingMarginSize marginSize = new PrintingMarginSize(
                metric ? PrintingStandards.DefaultMetricMarginInHundreths : PrintingStandards.DefaultEnglishMarginInHundreths);
            return new PrintingPaperSizeWithMargins(paperSize, marginSize);
        }

        private string CurrentEventDirectory()
        {
            if (controller == null || string.IsNullOrEmpty(controller.FileName)) {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            return Path.GetDirectoryName(controller.FileName) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private string CurrentEventBaseName()
        {
            if (controller == null || string.IsNullOrEmpty(controller.FileName)) {
                return "PurplePen";
            }

            return Path.GetFileNameWithoutExtension(controller.FileName);
        }

        private async Task<string?> PickOutputFolder(string? title = null)
        {
            FolderOpenViewModel folderOpenVm = new FolderOpenViewModel {
                Title = title,
                InitialDirectory = CurrentEventDirectory()
            };

            bool result = await Services.DialogService.ShowDialogAsync(folderOpenVm);
            return result ? folderOpenVm.SelectedFolder : null;
        }

        private async Task<bool> ConfirmOverwriteFiles(List<string> overwritingFiles)
        {
            if (overwritingFiles.Count == 0) {
                return true;
            }

            return await YesNoQuestion(
                "The following files already exist and will be overwritten:\n\n" +
                string.Join("\n", overwritingFiles) + "\n\nContinue?",
                false);
        }

        private static Id<Course>[] AllCourseIds(EventDB eventDB)
        {
            return QueryEvent.SortedCourseIds(eventDB, true);
        }

        /// <summary>
        /// Opens a generated output file in the platform default application.
        /// </summary>
        /// <param name="fileName">The file to open.</param>
        private async Task OpenFileWithDefaultApplication(string fileName)
        {
            try {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName) {
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex) {
                await ErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// Disposable placeholder for controller modes that do not own a UI object.
        /// </summary>
        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new NullDisposable();

            private NullDisposable()
            {
            }

            public void Dispose()
            {
            }
        }

        /// <summary>
        /// Executes the File/Create OCAD Files command.
        /// </summary>
        [RelayCommand]
        private async Task CreateOcadFiles()
        {
            if (controller == null || MapDisplay == null) { return; }

            bool success = false;

            MapFileFormatKind restrictToKind;  // restrict to outputting this kind of map.
            if (MapDisplay.MapType == MapType.OCAD) {
                restrictToKind = MapDisplay.MapVersion.kind;
            }
            else {
                restrictToKind = MapFileFormatKind.None;
            }

            OcadCreationSettings settings;
            if (ocadCreationSettingsPrevious != null)
            {
                settings = ocadCreationSettingsPrevious.Clone();
                if (restrictToKind != MapFileFormatKind.None & restrictToKind != ocadCreationSettingsPrevious.fileFormat.kind) {
                    settings.fileFormat = MapDisplay.MapVersion;
                }
            }
            else {
                // Default settings: creating in file directory, use format of the current map file.
                settings = new OcadCreationSettings();

                settings.fileDirectory = false;
                settings.mapDirectory = false;
                settings.outputDirectory = CurrentEventDirectory();
                settings.CourseIds = AllCourseIds(controller.GetEventDB());
                settings.AllCourses = true;
                if (MapDisplay.MapType == MapType.OCAD) {
                    settings.fileFormat = MapDisplay.MapVersion;
                }
                else {
                    settings.fileFormat = new MapFileFormat(MapFileFormatKind.OCAD, 8);
                }
            }

            CreateOcadFilesViewModel viewModel = new CreateOcadFilesViewModel();
            viewModel.Initialize(controller.GetEventDB(), restrictToKind, settings);
            bool result = await Services.DialogService.ShowDialogAsync(viewModel);
            if (!result) {
                return;
            }

            settings = viewModel.BuildSettings();

            // Get the correct purple color to use.
            FindPurple.GetPurpleColor(MapDisplay, controller.GetCourseAppearance(), out settings.colorOcadId, out settings.cyan, out settings.magenta, out settings.yellow, out settings.black, out settings.purpleOverprint);

            if (!await ConfirmOverwriteFiles(controller.OverwritingOcadFiles(settings))) {
                return;
            }

            List<string> warnings = controller.OcadFilesWarnings(settings);
            foreach (string warning in warnings) {
                await WarningMessage(warning);
            }

            ocadCreationSettingsPrevious = settings;
            success = controller.CreateOcadFiles(settings);

            if (MapDisplay.MapType == MapType.Bitmap) {
                await InfoMessage(MiscText.ClosePPBeforeLoadingOCAD);
            }

            // The Windows Store version doesn't install Roboto fonts into the system. So we may need to tell the user to install them.
            if (success) {
                if (controller.ShouldInstallRobotoFonts()) {
                    if (await YesNoQuestion(MiscText.AskInstallRobotoFonts, true)) {
                        bool installSucceeded = controller.InstallRobotoFonts();
                        if (!installSucceeded)
                            await ErrorMessage(MiscText.RobotoFontsInstallFailed);
                    }
                }
            }
        }

        /// <summary>
        /// Executes the File/Create Image Files command.
        /// </summary>
        [RelayCommand]
        private async Task CreateImageFiles()
        {
            if (controller == null) { return; }

            BitmapCreationSettings settings;
            if (bitmapCreationSettingsPrevious != null) {
                settings = bitmapCreationSettingsPrevious.Clone();
            }
            else {
                // Default settings: creating in file directory, use format of the current map file.
                settings = new BitmapCreationSettings();

                settings.fileDirectory = false;
                settings.mapDirectory = false;
                settings.outputDirectory = CurrentEventDirectory();
                settings.CourseIds = AllCourseIds(controller.GetEventDB());
                settings.AllCourses = true;
                settings.Dpi = 200;
                settings.ColorModel = ColorModel.CMYK;
                settings.ExportedBitmapKind = BitmapCreationSettings.BitmapKind.Png;
            }

            bool worldFileEnabled = controller.BitmapFilesCanCreateWorldFile();
            if (!worldFileEnabled) {
                settings.WorldFile = false;
            }

            CreateImageFilesViewModel viewModel = new CreateImageFilesViewModel();
            viewModel.Initialize(controller.GetEventDB(), settings, worldFileEnabled);
            bool result = await Services.DialogService.ShowDialogAsync(viewModel);
            if (!result) {
                return;
            }

            settings = viewModel.BuildSettings();

            if (!await ConfirmOverwriteFiles(controller.OverwritingBitmapFiles(settings))) {
                return;
            }

            bitmapCreationSettingsPrevious = settings;
            controller.CreateBitmapFiles(settings);
        }

        /// <summary>
        /// Executes the File/Create Route Gadget Files command.
        /// </summary>
        [RelayCommand]
        private async Task CreateRouteGadgetFiles()
        {
            if (controller == null) { return; }

            RouteGadgetCreationSettings settings;
            if (routeGadgetCreationSettingsPrevious != null)
                settings = routeGadgetCreationSettingsPrevious.Clone();
            else {
                // Default settings: creating in file directory, use format of the current map file.
                settings = new RouteGadgetCreationSettings();

                settings.fileDirectory = false;
                settings.mapDirectory = false;
                settings.outputDirectory = CurrentEventDirectory();
                settings.fileBaseName = CurrentEventBaseName();
            }

            CreateRouteGadgetViewModel viewModel = new CreateRouteGadgetViewModel();
            viewModel.Initialize(settings);
            bool result = await Services.DialogService.ShowDialogAsync(viewModel);
            if (!result) {
                return;
            }

            settings = viewModel.BuildSettings();

            if (!await ConfirmOverwriteFiles(controller.OverwritingRouteGadgetFiles(settings))) {
                return;
            }

            routeGadgetCreationSettingsPrevious = settings;
            controller.CreateRouteGadgetFiles(settings);
        }

        /// <summary>
        /// Executes the File/Export XML command.
        /// </summary>
        [RelayCommand]
        private async Task CreateXml()
        {
            if (controller == null || MapDisplay == null) { return; }

            // The default output for the XML is the same as the event file name, with xml extension.
            string xmlFileName = CurrentEventBaseName() + ".xml";

            FileSaveViewModel saveVm = new FileSaveViewModel {
                Title = "Create XML Interchange File",
                FileFilters = "IOF XML version 3.0|*.xml",
                DefaultExtension = "xml",
                InitialDirectory = CurrentEventDirectory(),
                SuggestedFileName = xmlFileName
            };

            if (await Services.DialogService.ShowDialogAsync(saveVm) && saveVm.SelectedFile != null) {
                controller.ExportXml(saveVm.SelectedFile, MapDisplay.MapBounds, 3);
            }
        }

        /// <summary>
        /// Executes the File/Export GPX command.
        /// </summary>
        [RelayCommand]
        private async Task CreateGpx()
        {
            if (controller == null) { return; }

            // First check and give immediate message if we can't do coordinate mapping.
            string message;
            if (!controller.CanExportGpxOrKml(out message)) {
                await ErrorMessage(message);
                return;
            }

            GpxCreationSettings settings;
            if (gpxCreationSettingsPrevious != null)
                settings = gpxCreationSettingsPrevious.Clone();
            else {
                // Default settings
                settings = new GpxCreationSettings();
                settings.CourseIds = AllCourseIds(controller.GetEventDB());
                settings.AllCourses = true;
            }

            settings.CourseIds ??= AllCourseIds(controller.GetEventDB());

            CreateGpxViewModel viewModel = new CreateGpxViewModel();
            viewModel.Initialize(controller.GetEventDB(), settings);
            bool result = await Services.DialogService.ShowDialogAsync(viewModel);
            if (!result) {
                return;
            }

            settings = viewModel.BuildSettings();

            FileSaveViewModel saveVm = new FileSaveViewModel {
                Title = "Create GPX File",
                FileFilters = "GPX file|*.gpx",
                DefaultExtension = "gpx",
                InitialDirectory = CurrentEventDirectory(),
                SuggestedFileName = CurrentEventBaseName() + ".gpx"
            };

            if (await Services.DialogService.ShowDialogAsync(saveVm) && saveVm.SelectedFile != null) {
                gpxCreationSettingsPrevious = settings;
                controller.ExportGpx(saveVm.SelectedFile, settings);
            }
        }

        /// <summary>
        /// Executes the File/Create KML Files command.
        /// </summary>
        [RelayCommand]
        private async Task CreateKmlFiles()
        {
            if (controller == null) { return; }

            // First check and give immediate message if we can't do coordinate mapping.
            string message;
            if (!controller.CanExportGpxOrKml(out message)) {
                await ErrorMessage(message);
                return;
            }

            ExportKmlSettings settings;
            if (exportKmlSettingsPrevious != null) {
                settings = exportKmlSettingsPrevious.Clone();
            }
            else {
                // Default settings: creating in file directory, use format of the current map file.
                settings = new ExportKmlSettings();

                settings.fileDirectory = true;
                settings.mapDirectory = false;
                settings.outputDirectory = CurrentEventDirectory();
                settings.CourseIds = AllCourseIds(controller.GetEventDB());
                settings.AllCourses = true;
            }

            CreateKmlFilesViewModel viewModel = new CreateKmlFilesViewModel();
            viewModel.Initialize(controller.GetEventDB(), settings);
            bool result = await Services.DialogService.ShowDialogAsync(viewModel);
            if (!result) {
                return;
            }

            settings = viewModel.BuildSettings();

            if (!await ConfirmOverwriteFiles(controller.OverwritingKmlFiles(settings))) {
                return;
            }

            exportKmlSettingsPrevious = settings;
            controller.CreateKmlFiles(settings);
        }

        /// <summary>
        /// Executes the File/Publish to Livelox command.
        /// </summary>
        [RelayCommand]
        private void PublishToLivelox()
        {
#if !PORTING
            LiveloxPublishSettings settings;
            if (liveloxPublishSettingsPrevious != null)
            {
                settings = liveloxPublishSettingsPrevious.Clone();
            }
            else
            {
                settings = new LiveloxPublishSettings();
            }

            var publishToLiveloxDialog = new PublishToLiveloxDialog(controller, symbolDB, settings);
            publishToLiveloxDialog.InitializeImportableEvent(this, call =>
            {
                // must invoke on UI thread
                this.InvokeOnUiThread(() => {
                    controller.EndProgressDialog();
                    if (call.Success)
                    {
                        publishToLiveloxDialog.ShowDialog(this);
                        liveloxPublishSettingsPrevious = publishToLiveloxDialog.PublishSettings;
                    }
                    else
                    {
                        MessageBox.Show(this, call.Exception?.Message, MiscText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    publishToLiveloxDialog.Dispose();
                });
            });
#endif
        }

        #endregion // Print and export commands

        #region Report commands

        /// <summary>
        /// Shows the Course Summary report.
        /// </summary>
        [RelayCommand]
        private void ShowCourseSummary()
        {
#if !PORTING
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateCourseSummaryReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm(WindowsUtil.RemoveHotkeyPrefix(courseSummaryMenu.Text), "", testReport, "ReportsCourseSummary.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
#endif
        }

        /// <summary>
        /// Shows the Control Cross-Reference report.
        /// </summary>
        [RelayCommand]
        private void ShowControlCrossref()
        {
#if !PORTING
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateCrossReferenceReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm(WindowsUtil.RemoveHotkeyPrefix(controlCrossrefMenu.Text), "", testReport, "ReportsControlCrossReference.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
#endif
        }

        /// <summary>
        /// Shows the Control and Leg Load report.
        /// </summary>
        [RelayCommand]
        private void ShowControlAndLegLoad()
        {
#if !PORTING
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateLoadReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm(WindowsUtil.RemoveHotkeyPrefix(controlAndLegLoadMenu.Text), "", testReport, "ReportsControlAndLegLoad.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
#endif
        }

        /// <summary>
        /// Shows the Leg Lengths report.
        /// </summary>
        [RelayCommand]
        private void ShowLegLengths()
        {
#if !PORTING
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateLegLengthReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm(WindowsUtil.RemoveHotkeyPrefix(legLengthsMenu.Text), "", testReport, "ReportsLegLengths.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
#endif
        }

        /// <summary>
        /// Shows the Event Audit report.
        /// </summary>
        [RelayCommand]
        private void ShowEventAudit()
        {
#if !PORTING
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateEventAuditReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm(WindowsUtil.RemoveHotkeyPrefix(eventAuditMenu.Text), "", testReport, "ReportsEventAudit.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
#endif
        }

        #endregion // Report commands

        #region Help and web commands

        /// <summary>
        /// Shows the help table of contents.
        /// </summary>
        [RelayCommand]
        private void HelpContents()
        {
#if !PORTING
            ShowHelp(HelpNavigator.TableOfContents, null);
#endif
        }

        /// <summary>
        /// Opens the translated help web site.
        /// </summary>
        [RelayCommand]
        private void HelpTranslated()
        {
#if !PORTING
            WindowsUtil.GoToWebPage(MiscText.TranslatedHelpWebSite);
#endif
        }

        /// <summary>
        /// Opens the main Purple Pen web site.
        /// </summary>
        [RelayCommand]
        private void OpenMainWebSite()
        {
#if !PORTING
            WindowsUtil.GoToWebPage("http://purple-pen.org");
#endif
        }

        /// <summary>
        /// Opens the Purple Pen support web site.
        /// </summary>
        [RelayCommand]
        private void OpenSupportWebSite()
        {
#if !PORTING
            WindowsUtil.GoToWebPage("http://purple-pen.org#support");
#endif
        }

        /// <summary>
        /// Opens the Purple Pen donate web site.
        /// </summary>
        [RelayCommand]
        private void OpenDonateWebSite()
        {
#if !PORTING
            WindowsUtil.GoToWebPage("http://purple-pen.org#donate");
#endif
        }

        /// <summary>
        /// Shows the About dialog.
        /// </summary>
        [RelayCommand]
        private async Task ShowAboutDialog()
        {
            AboutDialogViewModel aboutViewModel = new AboutDialogViewModel();
            await Services.DialogService.ShowDialogAsync(aboutViewModel);
        }

        /// <summary>
        /// Shows the Switch Language dialog and applies the selected language.
        /// </summary>
        [RelayCommand]
        private async Task ShowSwitchLanguageDialog()
        {
            string currentCode = Services.UILanguage.LanguageCode;
            SwitchLanguageDialogViewModel vm = new SwitchLanguageDialogViewModel(currentCode, SwitchLanguageDialogViewModel.CreateDefaultLanguages());
            bool result = await Services.DialogService.ShowDialogAsync(vm);

            if (result && vm.SelectedLanguage != null) {
                Services.UILanguage.LanguageCode = vm.SelectedLanguage.Code;
            }
        }

        #endregion // Help and web commands

        #region Localization commands

        /// <summary>
        /// Executes the Translate/Add Description Language command.
        /// </summary>
        [RelayCommand]
        private void AddDescriptionLanguage()
        {
#if !PORTING
            DebugUI.NewLanguage newLanguageDialog = new NewLanguage(symbolDB);

            if (newLanguageDialog.ShowDialog(this) == DialogResult.OK) {
                SymbolLanguage symLanguage = new SymbolLanguage(newLanguageDialog.LanguageName, newLanguageDialog.LangId, newLanguageDialog.PluralNouns,
                    newLanguageDialog.PluralModifiers, newLanguageDialog.GenderModifiers,
                    newLanguageDialog.GenderModifiers ? newLanguageDialog.Genders.Split(new string[] {",", " "}, StringSplitOptions.RemoveEmptyEntries) : new string[0],
                    newLanguageDialog.CaseModifiers,
                    newLanguageDialog.CaseModifiers ? newLanguageDialog.Cases.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries) : new string[0]);
                controller.AddDescriptionLanguage(symLanguage, newLanguageDialog.CopyFromLangId);
                controller.SetDescriptionLanguage(symLanguage.LangId);
            }
#endif
        }

        /// <summary>
        /// Executes the Translate/Add Translated Texts command.
        /// </summary>
        [RelayCommand]
        private void AddTranslatedTexts()
        {
#if !PORTING
            // Initialize the dialog
            CustomSymbolText dialog = new CustomSymbolText(symbolDB, true);
            dialog.LangId = controller.GetDescriptionLanguage();

            // Show the dialog.
            DialogResult result = dialog.ShowDialog(this);

            // Apply the changes
            if (result == DialogResult.OK) {
                controller.AddDescriptionTexts(dialog.CustomSymbolTexts, dialog.SymbolNames);
                controller.SetDescriptionLanguage(dialog.LangId);
            }

            dialog.Dispose();
#endif
        }

        /// <summary>
        /// Executes the Translate/Merge Symbols command.
        /// </summary>
        [RelayCommand]
        private void MergeSymbols()
        {
#if !PORTING
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = ".xml";
            if (openFile.ShowDialog() == DialogResult.OK) {
                string filename = openFile.FileName;
                string langId = Microsoft.VisualBasic.Interaction.InputBox("Language code to import", "Merge Symbols.xml", null, 0, 0);
                controller.MergeSymbolsXml(filename, langId);
            }

            openFile.Dispose();
#endif
        }

        #endregion // Localization commands

        #region Debug commands

        /// <summary>
        /// Shows the Symbol Browser debug dialog.
        /// </summary>
        [RelayCommand]
        private void ShowSymbolBrowser()
        {
#if !PORTING
            SymbolBrowser symbolBrowser = new SymbolBrowser();
            symbolBrowser.Initialize(symbolDB);
            symbolBrowser.ShowDialog();
            symbolBrowser.Dispose();
#endif
        }

        /// <summary>
        /// Shows the Description Browser debug dialog.
        /// </summary>
        [RelayCommand]
        private void ShowDescriptionBrowser()
        {
#if !PORTING
            DescriptionBrowser browser = new DescriptionBrowser();
            browser.Initialize(controller.GetEventDB(), symbolDB);
            browser.ShowDialog();
            browser.Dispose();
#endif
        }

        /// <summary>
        /// Shows the Control Tester debug dialog.
        /// </summary>
        [RelayCommand]
        private void ShowControlTester()
        {
#if !PORTING
            ControlTester controlTester = new ControlTester();
            controlTester.Initialize(controller.GetEventDB(), symbolDB);
            controlTester.ShowDialog();
            controlTester.Dispose();
#endif
        }

        /// <summary>
        /// Shows the Map Tester debug dialog.
        /// </summary>
        [RelayCommand]
        private void ShowMapTester()
        {
#if !PORTING
            MapTester mapTester = new MapTester();
            mapTester.ShowDialog();
            mapTester.Dispose();
#endif
        }

        /// <summary>
        /// Shows the Course Selector Tester debug dialog.
        /// </summary>
        [RelayCommand]
        private void ShowCourseSelectorTester()
        {
#if !PORTING
            new CourseSelectorTestForm(controller.GetEventDB()).ShowDialog(this);
#endif
        }

        /// <summary>
        /// Shows the Dot Grid Tester debug dialog.
        /// </summary>
        [RelayCommand]
        private void ShowDotGridTester()
        {
#if !PORTING
            new DotGridTester().ShowDialog(this);
#endif
        }

        /// <summary>
        /// Shows the Dump OCAD File debug dialog.
        /// </summary>
        [RelayCommand]
        private void DumpOcadFile()
        {
#if !PORTING
            OpenFileDialog openOcadFileDialog = new OpenFileDialog();
            openOcadFileDialog.Filter = "OCAD files|*.ocd|All files|*.*";
            openOcadFileDialog.FilterIndex = 1;
            openOcadFileDialog.DefaultExt = "ocd";

            DialogResult result = openOcadFileDialog.ShowDialog(this);
            if (result != DialogResult.OK)
                return;
            string ocadFile = openOcadFileDialog.FileName;

            SaveFileDialog saveDumpFileDialog = new SaveFileDialog();
            saveDumpFileDialog.Filter = "Test file|*.txt";
            saveDumpFileDialog.FilterIndex = 1;
            saveDumpFileDialog.DefaultExt = "txt";

            result = saveDumpFileDialog.ShowDialog(this);
            if (result != DialogResult.OK)
                return;
            string dumpFile = saveDumpFileDialog.FileName;

            using (TextWriter writer = new StreamWriter(dumpFile)) {
                PurplePen.MapModel.DebugCode.OcadDump dumper = new PurplePen.MapModel.DebugCode.OcadDump();
                dumper.DumpFile(ocadFile, writer);
            }
#endif
        }

        /// <summary>
        /// Shows the Report Tester debug dialog.
        /// </summary>
        [RelayCommand]
        private void ShowReportTester()
        {
#if !PORTING
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateTestReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm("Test Report", "", testReport, "PurplePenWindow.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
#endif
        }

        /// <summary>
        /// Shows the Font Metrics debug dialog.
        /// </summary>
        [RelayCommand]
        private void ShowFontMetrics()
        {
#if !PORTING
            ShowFontMetrics fontMetricsDialog = new ShowFontMetrics(new GDIPlus_TextMetrics());

            fontMetricsDialog.ShowDialog(this);
            fontMetricsDialog.Dispose();
#endif
        }

        /// <summary>
        /// Shows the Missing Translations debug dialog.
        /// </summary>
        [RelayCommand]
        private void ShowMissingTranslations()
        {
#if !PORTING
            UntranslatedSymbolTexts untranslatedSymbolTexts = new UntranslatedSymbolTexts();
            string report = untranslatedSymbolTexts.ReportOnUntranslatedSymbolTexts(symbolDB);

            DebugTextForm debugTextForm = new DebugTextForm("Missing Translations", report);
            debugTextForm.ShowDialog(this);
            debugTextForm.Dispose();
#endif
        }

        /// <summary>
        /// Intentional crash for testing error handling.
        /// </summary>
        [RelayCommand]
        private void TriggerCrash()
        {
#if !PORTING
            int x = 0;
            int y = 5 / x;
#endif
        }

        /// <summary>
        /// Test: shows a message box with OK button and Information icon.
        /// </summary>
        [RelayCommand]
        private async Task TestMessageBoxOk()
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = "This is an informational message with an OK button.",
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Information
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        /// <summary>
        /// Test: shows a message box with OK/Cancel buttons and Warning icon.
        /// </summary>
        [RelayCommand]
        private async Task TestMessageBoxOkCancel()
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = "This is a warning message with OK and Cancel buttons.",
                Buttons = MessageBoxButtons.OkCancel,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Warning
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        /// <summary>
        /// Test: shows a message box with Yes/No buttons and Question icon.
        /// </summary>
        [RelayCommand]
        private async Task TestMessageBoxYesNo()
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = "This is a question message with Yes and No buttons. Do you want to proceed?",
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = MessageBoxButton.Yes,
                Icon = MessageBoxIcon.Question
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        /// <summary>
        /// Test: shows a message box with Yes/No/Cancel buttons and Error icon.
        /// </summary>
        [RelayCommand]
        private async Task TestMessageBoxYesNoCancel()
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = "This is an error message with Yes, No, and Cancel buttons.",
                Buttons = MessageBoxButtons.YesNoCancel,
                DefaultButton = MessageBoxButton.Yes,
                Icon = MessageBoxIcon.Error
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        #endregion // Debug commands

    }
}

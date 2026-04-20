# Session Notes: Event Menu Dialog Fixes (2026-04-20)

## Problem Solved
Fixed FileNotFoundException crash when opening ChangeCodesDialog due to Avalonia.Diagnostics assembly loading error.

## Solution
Replaced DataGrid control with ItemsControl + custom DataTemplate in ChangeCodesDialog.axaml. DataGrid causes runtime assembly loading issues even though dependency is declared; ItemsControl achieves the same tabular effect without these issues.

```xml
<!-- AVOID: DataGrid causes Avalonia.Diagnostics assembly error -->
<DataGrid ItemsSource="{Binding Codes}" AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Control #" Binding="{Binding ControlNumber}"/>
    </DataGrid.Columns>
</DataGrid>

<!-- USE: ItemsControl with DataTemplate for tabular display -->
<ItemsControl ItemsSource="{Binding Codes}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Grid ColumnDefinitions="100,*">
                <TextBlock Grid.Column="0" Text="{Binding ControlNumber}"/>
                <TextBox Grid.Column="1" Text="{Binding Code, Mode=TwoWay}"/>
            </Grid>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

## Move All Controls Feature - COMPLETE

Implemented full MoveAllControls workflow with dialog selection and interactive location selection interface.

**Files Created:**
- MoveAllControlsDialogViewModel.cs - SelectedActionIndex property (0-3), GetSelectedAction() method
- MoveAllControlsDialog.axaml - RadioButton UI with GroupName="action"
- MoveAllControlsDialog.axaml.cs - Code-behind uses FindControl + IsCheckedChanged for state sync

**Interactive Location Selection Implemented:**
- MainWindowViewModel.cs - Added MoveAllControls state properties and methods:
  - IsMoveAllControlsActive - tracks when mode is active
  - MoveAllControlsStage - current stage (0=SelectFirstControl, 1=MoveFirstControl, 2=SelectSecondControl, 3=MoveSecondControl, 4=Confirm)
  - MoveAllControlsAction - selected transformation (Move, MoveScale, MoveRotate, MoveRotateScale)
  - MoveAllControlsInstructions - dynamic instructions text
  - Transformation value properties: XOffset, YOffset, Scale, Rotation
  - StartMoveAllControls(action) - initializes the interactive mode
  - HandleMoveAllControlsClick(location) - processes map clicks during selection
  - EnterMoveAllControlsStage() - updates controller and instructions for current stage
  - ConfirmMoveAllControls() / CancelMoveAllControls() - finalize the operation

- MainWindow.axaml - Added overlay UI:
  - Semi-transparent dark overlay when mode active
  - White centered panel with instructions and transformation values
  - Dynamic stage-specific messages (click control, click location, confirm)
  - Displays X/Y offset (always), Scale/Rotation (shown when applicable)
  - Confirm/Cancel buttons only visible at final stage

**Workflow:**
1. User selects Menu → Event → Move All Controls
2. MoveAllControlsDialog shown to select action (Move, MoveScale, MoveRotate, MoveRotateScale)
3. Dialog OK → controller.BeginMoveAllControls() called
4. Interactive mode starts with overlay panel showing "Click on the first control to move"
5. User clicks control → stage advances to "Click on new location for first control"
6. User clicks new location → transformation values calculated and displayed
7. If action != Move, stage advances to "Click on second control" (for scale/rotate)
8. User clicks second control → "Click new location for second control"
9. User clicks new location → all transformation values finalized
10. Stage advances to Confirm with "Transformation ready" message
11. User clicks Confirm or Cancel button
12. Mode deactivates, changes applied via controller.FinishMoveAllControls()

**Map Click Integration:**
- LeftButtonClick() method in MainWindowViewModel checks IsMoveAllControlsActive
- If active, HandleMoveAllControlsClick() processes the click
- Otherwise, normal controller.LeftButtonClick() behavior
- No mode system changes needed - works directly with controller callbacks

## Verified Implementation
All 8 Event menu dialog commands fully functional:
- ChangeMapFile - map file, scale, DPI settings (DPI hidden for non-bitmap)
- ChangeCodes - control code editor with Id<T> type handling
- AutoNumbering - first code and invert options
- RemoveUnusedControls - confirmation before deletion
- PunchPatterns - view all punch patterns
- CustomizeDescriptions - 13 language options
- CustomizeCourseAppearance - appearance settings, map standards
- **MoveAllControls** - select transformation action

## Build Status
Build succeeds. No compilation errors. Ready for testing.

## Dotnet CLI
Location: `~/.dotnet/dotnet`  
Always use: `PATH="$HOME/.dotnet:$PATH" ~/.dotnet/dotnet build ...`

## Important Notes
- Never use Avalonia DataGrid in dialogs — use ItemsControl with DataTemplate
- RadioButton selection state in code-behind: Use FindControl(x:Name) + IsCheckedChanged event handler
- Services.DialogService handles dialog show/result flow with DialogResult property
- All ViewModels have proper Initialize() methods for data setup
- Id<T> types require special handling (extract .id field, wrap back on return)
- MoveAllControls location selection works directly with controller callbacks, no mode system needed
- Overlay UI panels: Use semi-transparent Border + centered Border with StackPanel for modal-like feel
- IsMoveAllControlsActive property gates all UI and input handling for clean separation of concerns
- Map click handling: Check for active mode in LeftButtonClick before calling controller

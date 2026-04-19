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
using AvUtil;
using PurplePen;
using PurplePen.ViewModels;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.ComponentModel;
using System.Threading;

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
    }
}

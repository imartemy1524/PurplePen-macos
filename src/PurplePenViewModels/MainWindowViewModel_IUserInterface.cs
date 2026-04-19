// IUserInterface implementation part of MainWindowViewModel.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace PurplePen.ViewModels
{
    public partial class MainWindowViewModel: IUserInterface
    {
        public void Initialize(Controller controller, SymbolDB symbolDB)
        {
            this.controller = controller;
            this.symbolDB = symbolDB;

            DescriptionViewerViewModel.SymbolDB = symbolDB;
            DescriptionViewerViewModel.Controller = controller;
            CoursePartBannerViewModel.Controller = controller;
        }

        public Size Size => new Size(0, 0);

        public void QueueIdleEvent()
        {
            Services.ServiceProvider.GetRequiredService<IApplicationIdleService>().QueueIdleEvent();
        }

        public void PostDelayedAction(Action action)
        {
            Services.ServiceProvider.GetRequiredService<IPostMessage>().PostMessage(action);
        }

        public async Task InfoMessage(string message)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Information
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        public async Task WarningMessage(string message)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Warning
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        public async Task ErrorMessage(string message)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Error
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        public async Task<bool> OKCancelMessage(string message, bool okDefault)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.OkCancel,
                DefaultButton = okDefault ? MessageBoxButton.Ok : MessageBoxButton.Cancel,
                Icon = MessageBoxIcon.Information
            };
            await Services.DialogService.ShowDialogAsync(vm);
            return vm.ChosenButton == MessageBoxButton.Ok;
        }

        public async Task<YesNoCancel> YesNoCancelQuestion(string message, bool yesDefault)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.YesNoCancel,
                DefaultButton = yesDefault ? MessageBoxButton.Yes : MessageBoxButton.No,
                Icon = MessageBoxIcon.Question
            };
            await Services.DialogService.ShowDialogAsync(vm);
            return ConvertMessageBoxButtonToYesNoCancel(vm.ChosenButton);
        }

        public async Task<bool> YesNoQuestion(string message, bool yesDefault)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = yesDefault ? MessageBoxButton.Yes : MessageBoxButton.No,
                Icon = MessageBoxIcon.Question
            };
            await Services.DialogService.ShowDialogAsync(vm);
            return vm.ChosenButton == MessageBoxButton.Yes;
        }

        public async Task<YesNoCancel> MovingSharedControl(string controlCode, string otherCourses)
        {
            string message = string.Format("Control {0} is used by other courses:\n\n{1}\n\nMove the shared control?", controlCode, otherCourses);
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.YesNoCancel,
                DefaultButton = MessageBoxButton.Yes,
                Icon = MessageBoxIcon.Question
            };
            await Services.DialogService.ShowDialogAsync(vm);
            return ConvertMessageBoxButtonToYesNoCancel(vm.ChosenButton);
        }

        public void ShowProgressDialog(bool knownDuration, Action onCancelPressed)
        {
            // TODO: replace this no-op with an Avalonia progress dialog.
        }

        public bool UpdateProgressDialog(string info, double fractionDone)
        {
            return false;
        }

        public void EndProgressDialog()
        {
            // TODO: replace this no-op with an Avalonia progress dialog.
        }

        public string GetOpenFileName()
        {
            return null!;
        }

        public async Task<bool> FindMissingMapFile(string missingMapFile)
        {
            if (controller == null)
                return false;

            FileOpenSingleViewModel fileOpenVm = new FileOpenSingleViewModel {
                Title = Path.GetFileName(missingMapFile),
                InitialDirectory = Path.GetDirectoryName(missingMapFile),
                FileFilters = "All map files|*.ocd;*.omap;*.xmap;*.pdf;*.jpeg;*.jpg;*.tiff;*.tif;*.bmp;*.png;*.gif|OCAD files (*.ocd)|*.ocd|Open Orienteering Mapper files|*.omap;*.xmap|PDF files (*.pdf)|*.pdf|Image files|*.jpeg;*.jpg;*.tiff;*.tif;*.bmp;*.png;*.gif|All files|*.*",
                InitialFileFilterIndex = 1
            };

            bool result = await Services.DialogService.ShowDialogAsync(fileOpenVm);
            if (!result || fileOpenVm.SelectedFile == null)
                return false;

            bool valid = CoreMapUtil.ValidateMapFile(fileOpenVm.SelectedFile, out float scale, out float dpi, out Size bitmapSize, out RectangleF mapBounds, out MapType mapType, out int? lowerPurpleLayer, out string errorMessageText);
            if (!valid) {
                await ErrorMessage(errorMessageText);
                return false;
            }

            controller.ChangeMapFile(mapType, fileOpenVm.SelectedFile, scale, dpi);
            return true;
        }

        public bool GetCurrentLocation(out PointF location, out float pixelSize)
        {
#if PORTING
            // TODO: get correct pixelSize.
            if (MouseLocationInMap.HasValue) {
                location = MouseLocationInMap.Value;
                pixelSize = 0.1F;
                return true;
            }
            else {
                location = new PointF();
                pixelSize = 0.1F;
                return false;
            }
#endif
        }

        public void InitiateMapDragging(PointF initialPos, PointerButton buttonEnd)
        {
            //throw new NotImplementedException();
        }

        public int LogicalToDeviceUnits(int value)
        {
            return value;
        }


        public void ShowTopologyView()
        {
            // Topology view is not ported to Avalonia yet.
        }

        private static YesNoCancel ConvertMessageBoxButtonToYesNoCancel(MessageBoxButton button)
        {
            switch (button) {
            case MessageBoxButton.Yes:
                return YesNoCancel.Yes;
            case MessageBoxButton.No:
                return YesNoCancel.No;
            default:
                return YesNoCancel.Cancel;
            }
        }

    }
}

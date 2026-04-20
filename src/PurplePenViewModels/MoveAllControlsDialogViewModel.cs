using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Move All Controls dialog.
    /// Allows selection of how to transform all controls (move, scale, rotate).
    /// </summary>
    public partial class MoveAllControlsDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int selectedActionIndex = 0; // 0=Move, 1=MoveScale, 2=MoveRotate, 3=MoveRotateScale

        [ObservableProperty]
        private bool dialogResult = false;

        public MoveAllControlsDialogViewModel()
        {
        }

        public MoveAllControlsAction GetSelectedAction()
        {
            return SelectedActionIndex switch
            {
                0 => MoveAllControlsAction.Move,
                1 => MoveAllControlsAction.MoveScale,
                2 => MoveAllControlsAction.MoveRotate,
                3 => MoveAllControlsAction.MoveRotateScale,
                _ => MoveAllControlsAction.Move
            };
        }

        [RelayCommand]
        private void Ok()
        {
            DialogResult = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false;
        }
    }
}

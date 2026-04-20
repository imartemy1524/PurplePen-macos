using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Customize Course Appearance dialog.
    /// Allows customization of how courses are rendered.
    /// </summary>
    public partial class CustomizeCourseAppearanceDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private float controlCircleSize = 1.0f;

        [ObservableProperty]
        private float lineWidth = 1.0f;

        [ObservableProperty]
        private float numberHeight = 1.0f;

        [ObservableProperty]
        private float centerDotDiameter = 0.0f;

        [ObservableProperty]
        private bool numberBold = false;

        [ObservableProperty]
        private bool numberRoboto = true;

        [ObservableProperty]
        private float autoLegGapSize = 3.5f;

        [ObservableProperty]
        private string purpleColorBlend = "Blend";

        [ObservableProperty]
        private bool descriptionsPurple = false;

        [ObservableProperty]
        private string mapStandard = "2000";

        [ObservableProperty]
        private ObservableCollection<string> mapStandardOptions = new();

        [ObservableProperty]
        private bool dialogResult = false;

        public CustomizeCourseAppearanceDialogViewModel()
        {
            MapStandardOptions.Add("ISOM 2000");
            MapStandardOptions.Add("ISOM 2017");
            MapStandardOptions.Add("ISOM Sprint 2019");
        }

        public void Initialize(CourseAppearance appearance)
        {
            ControlCircleSize = appearance.controlCircleSize;
            LineWidth = appearance.lineWidth;
            NumberHeight = appearance.numberHeight;
            CenterDotDiameter = appearance.centerDotDiameter;
            NumberBold = appearance.numberBold;
            NumberRoboto = appearance.numberRoboto;
            AutoLegGapSize = appearance.autoLegGapSize;
            PurpleColorBlend = appearance.purpleColorBlend.ToString();
            DescriptionsPurple = appearance.descriptionsPurple;
            MapStandard = appearance.mapStandard;
        }

        public CourseAppearance GetUpdatedAppearance(CourseAppearance original)
        {
            original.controlCircleSize = ControlCircleSize;
            original.lineWidth = LineWidth;
            original.numberHeight = NumberHeight;
            original.centerDotDiameter = CenterDotDiameter;
            original.numberBold = NumberBold;
            original.numberRoboto = NumberRoboto;
            original.autoLegGapSize = AutoLegGapSize;
            original.descriptionsPurple = DescriptionsPurple;
            return original;
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

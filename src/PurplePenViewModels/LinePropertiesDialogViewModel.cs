// LinePropertiesDialogViewModel.cs
//
// ViewModel for the Line Properties dialog.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

#if PORTING
#endif

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Line Properties dialog.
    /// </summary>
    public partial class LinePropertiesDialogViewModel : ViewModelBase
    {
        private bool syncingColor = false;

        [ObservableProperty]
        private SpecialColor color = SpecialColor.Black;

        [ObservableProperty]
        private int selectedLineKind = 0;  // LineKind.Single

        [ObservableProperty]
        private bool showLineKind = true;

        [ObservableProperty]
        private bool showRadius = true;

        [ObservableProperty]
        private float lineWidth = 0.35f;

        [ObservableProperty]
        private float gapSize = 1.0f;

        [ObservableProperty]
        private float dashSize = 2.0f;

        [ObservableProperty]
        private float cornerRadius = 0f;

        [ObservableProperty]
        private string explanation = "";

        [ObservableProperty]
        private bool dialogResult = false;

        [ObservableProperty]
        private int selectedColorIndex = 0;  // 0=Black, 1=Purple

        public ObservableCollection<string> LineKindOptions { get; } = new()
        {
            "Single",
            "Double",
            "Dashed"
        };

        public ObservableCollection<string> ColorOptions { get; } = new()
        {
            "Black",
            "Purple"
        };

        public LineKind LineKind
        {
            get => (LineKind)SelectedLineKind;
            set => SelectedLineKind = (int)value;
        }

        public void Initialize(string title, string explanation, string helpTopic, SpecialColor purpleColor, CourseAppearance appearance)
        {
            Explanation = explanation;
            Color = purpleColor;
            SelectedColorIndex = (Color.Kind == SpecialColor.ColorKind.Black) ? 0 : 1;
            SelectedLineKind = 0;  // Default to Single
        }

        partial void OnSelectedColorIndexChanged(int value)
        {
            if (!syncingColor)
            {
                syncingColor = true;
                Color = (value == 0) ? SpecialColor.Black : SpecialColor.UpperPurple;
                syncingColor = false;
            }
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

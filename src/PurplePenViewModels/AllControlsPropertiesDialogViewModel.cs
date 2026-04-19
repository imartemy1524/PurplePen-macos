// AllControlsPropertiesDialogViewModel.cs
//
// ViewModel for the All Controls Properties dialog.
// Holds the shared printing scale and description appearance values.
//
// Migrated from WinForms PurplePen/AllControlsProperties.cs.

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the All Controls Properties dialog.
    /// </summary>
    public partial class AllControlsPropertiesDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Text entered or selected for the printing scale.
        /// </summary>
        [ObservableProperty]
        private string printScaleText = "";

        /// <summary>
        /// Available print scales for the drop-down.
        /// </summary>
        public ObservableCollection<string> AvailablePrintScales { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Selected description appearance.
        /// </summary>
        [ObservableProperty]
        private DescriptionKind descKind = DescriptionKind.Symbols;

        /// <summary>
        /// Gets or sets the printing scale as a float.
        /// </summary>
        public float PrintScale
        {
            get {
                if (float.TryParse(PrintScaleText, out float scale))
                    return scale;
                return 0;
            }
            set {
                PrintScaleText = value.ToString();
            }
        }

        /// <summary>
        /// Initializes the available print scales from the map scale.
        /// </summary>
        public void InitializePrintScales(float mapScale)
        {
            AvailablePrintScales.Clear();
            foreach (int scale in MapUtil.PrintScaleList(mapScale)) {
                AvailablePrintScales.Add(scale.ToString());
            }
        }
    }
}

// VariationOptionViewModel.cs
//
// Represents one selectable course variation.

using CommunityToolkit.Mvvm.ComponentModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// Represents one selectable variation in the choose variations dialog.
    /// </summary>
    public partial class VariationOptionViewModel : ViewModelBase
    {
        /// <summary>
        /// Variation code shown to the user.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// True when this variation is selected.
        /// </summary>
        [ObservableProperty]
        private bool isSelected;

        /// <summary>
        /// Creates a variation option.
        /// </summary>
        /// <param name="code">The variation code.</param>
        /// <param name="isSelected">Initial selected state.</param>
        public VariationOptionViewModel(string code, bool isSelected)
        {
            Code = code;
            IsSelected = isSelected;
        }

        /// <summary>
        /// Parameterless constructor for the designer.
        /// </summary>
        public VariationOptionViewModel()
        {
            Code = "";
        }
    }
}

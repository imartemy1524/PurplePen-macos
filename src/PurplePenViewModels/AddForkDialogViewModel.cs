// AddForkDialogViewModel.cs
//
// ViewModel for choosing what kind of variation to add to a course.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using PurplePen;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Add Variation dialog.
    /// </summary>
    public partial class AddForkDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// True when the dialog is configured for a loop instead of a fork.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFork))]
        [NotifyPropertyChangedFor(nameof(SummaryText))]
        private bool isLoop;

        /// <summary>
        /// True when the dialog is configured for a fork.
        /// </summary>
        public bool IsFork {
            get { return !IsLoop; }
            set { IsLoop = !value; }
        }

        /// <summary>
        /// The number of branches selected by the user.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SummaryText))]
        private int numberOfBranches = 2;

        /// <summary>
        /// Available branch counts shown in the combo box.
        /// </summary>
        public ObservableCollection<int> NumberBranchOptions { get; } = new ObservableCollection<int>(Enumerable.Range(2, 9));

        /// <summary>
        /// Summary text that mirrors the legacy WinForms dialog.
        /// </summary>
        public string SummaryText {
            get {
                if (IsLoop) {
                    return string.Format(MiscText.LoopSummary, NumberOfBranches + 1, Util.Factorial(NumberOfBranches));
                }
                else {
                    return string.Format(MiscText.ForkSummary, string.Join(", ", PossibleRelayParticipants(NumberOfBranches)));
                }
            }
        }

        /// <summary>
        /// Parameterless constructor for the designer.
        /// </summary>
        public AddForkDialogViewModel()
        {
            IsLoop = false;
            NumberOfBranches = 2;
        }

        /// <summary>
        /// Gets the selected dialog result in controller-friendly form.
        /// </summary>
        public bool Loop => IsLoop;

        /// <summary>
        /// Returns the branch count for the selected variation type.
        /// </summary>
        public int BranchCount => NumberOfBranches;

        /// <summary>
        /// Returns the set of team counts that can evenly distribute a fork.
        /// </summary>
        private static System.Collections.Generic.IEnumerable<int> PossibleRelayParticipants(int numForks)
        {
            for (int i = 2; i <= 20; ++i) {
                if (i % numForks == 0) {
                    yield return i;
                }
            }
        }
    }
}

// SelectVariationsViewModel.cs
//
// ViewModel for choosing which course variations to export.

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for selecting variation export behavior for one course.
    /// </summary>
    public partial class SelectVariationsViewModel : ViewModelBase
    {
        private EventDB? eventDB;
        private Id<Course> courseId;
        private int? firstTeam;
        private int? lastTeam;

        /// <summary>
        /// Individual variation choices.
        /// </summary>
        public ObservableCollection<VariationOptionViewModel> VariationOptions { get; } = new ObservableCollection<VariationOptionViewModel>();

        /// <summary>
        /// Variation mode: 0 = separate variations, 1 = relay legs, 2 = combined.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCombinedMode))]
        [NotifyPropertyChangedFor(nameof(IsSeparateVariationMode))]
        [NotifyPropertyChangedFor(nameof(IsRelayLegModeAvailable))]
        [NotifyPropertyChangedFor(nameof(IsRelayLegModeUnavailable))]
        [NotifyPropertyChangedFor(nameof(IsVariationListVisible))]
        private int variationModeIndex;

        /// <summary>
        /// True when only specific variation codes should be exported.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsVariationListVisible))]
        private bool selectIndividualVariations;

        /// <summary>
        /// First relay team to include.
        /// </summary>
        [ObservableProperty]
        private int firstTeamValue = 1;

        /// <summary>
        /// Last relay team to include.
        /// </summary>
        [ObservableProperty]
        private int lastTeamValue = 1;

        /// <summary>
        /// Minimum available team number.
        /// </summary>
        [ObservableProperty]
        private int minTeamValue = 1;

        /// <summary>
        /// Maximum available team number.
        /// </summary>
        [ObservableProperty]
        private int maxTeamValue = 1;

        /// <summary>
        /// Number of relay teams assigned to the course.
        /// </summary>
        [ObservableProperty]
        private int teamCount;

        /// <summary>
        /// True when all variations are combined on one map.
        /// </summary>
        public bool IsCombinedMode => VariationModeIndex == 2;

        /// <summary>
        /// True when each variation is exported separately.
        /// </summary>
        public bool IsSeparateVariationMode => VariationModeIndex == 0;

        /// <summary>
        /// True when relay leg mode is selected and relay teams exist.
        /// </summary>
        public bool IsRelayLegModeAvailable => VariationModeIndex == 1 && lastTeam.HasValue;

        /// <summary>
        /// True when relay leg mode is selected and no relay teams exist.
        /// </summary>
        public bool IsRelayLegModeUnavailable => VariationModeIndex == 1 && !lastTeam.HasValue;

        /// <summary>
        /// True when individual variation checkboxes should be visible.
        /// </summary>
        public bool IsVariationListVisible => IsSeparateVariationMode && SelectIndividualVariations;

        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        /// <param name="eventDB">The event database.</param>
        /// <param name="courseId">The course whose variations are being selected.</param>
        /// <param name="variationChoices">Existing variation choices.</param>
        public void Initialize(EventDB eventDB, Id<Course> courseId, VariationChoices variationChoices)
        {
            this.eventDB = eventDB;
            this.courseId = courseId;

            Course course = eventDB.GetCourse(courseId);
            if (course.relaySettings.relayTeams > 0) {
                firstTeam = course.relaySettings.firstTeamNumber;
                lastTeam = firstTeam + course.relaySettings.relayTeams - 1;
                MinTeamValue = firstTeam.Value;
                MaxTeamValue = lastTeam.Value;
                FirstTeamValue = firstTeam.Value;
                LastTeamValue = lastTeam.Value;
                TeamCount = lastTeam.Value - firstTeam.Value + 1;
            }
            else {
                firstTeam = null;
                lastTeam = null;
                TeamCount = 0;
            }

            HashSet<string> selectedVariations = new HashSet<string>(variationChoices.ChosenVariations ?? new List<string>());
            VariationOptions.Clear();
            foreach (VariationInfo variationInfo in QueryEvent.GetAllVariations(eventDB, courseId)) {
                bool isSelected = variationChoices.Kind != VariationChoices.VariationChoicesKind.ChosenVariations ||
                                  selectedVariations.Contains(variationInfo.CodeString);
                VariationOptions.Add(new VariationOptionViewModel(variationInfo.CodeString, isSelected));
            }

            switch (variationChoices.Kind) {
                case VariationChoices.VariationChoicesKind.Combined:
                    VariationModeIndex = 2;
                    break;
                case VariationChoices.VariationChoicesKind.ChosenTeams:
                    VariationModeIndex = 1;
                    if (firstTeam.HasValue && lastTeam.HasValue) {
                        FirstTeamValue = ClampTeam(variationChoices.FirstTeam);
                        LastTeamValue = ClampTeam(variationChoices.LastTeam);
                    }
                    break;
                case VariationChoices.VariationChoicesKind.ChosenVariations:
                    VariationModeIndex = 0;
                    SelectIndividualVariations = true;
                    break;
                default:
                    VariationModeIndex = 0;
                    SelectIndividualVariations = false;
                    break;
            }
        }

        /// <summary>
        /// Builds the variation choices represented by the dialog.
        /// </summary>
        /// <returns>The selected variation choices.</returns>
        public VariationChoices BuildVariationChoices()
        {
            VariationChoices result = new VariationChoices();

            switch (VariationModeIndex) {
                case 0:
                    if (SelectIndividualVariations) {
                        result.Kind = VariationChoices.VariationChoicesKind.ChosenVariations;
                        result.ChosenVariations = VariationOptions.Where(x => x.IsSelected).Select(x => x.Code).ToList();
                    }
                    else {
                        result.Kind = VariationChoices.VariationChoicesKind.AllVariations;
                    }
                    break;

                case 1:
                    if (lastTeam.HasValue) {
                        result.Kind = VariationChoices.VariationChoicesKind.ChosenTeams;
                        result.FirstTeam = FirstTeamValue;
                        result.LastTeam = LastTeamValue < FirstTeamValue ? FirstTeamValue : LastTeamValue;
                    }
                    else {
                        result.Kind = VariationChoices.VariationChoicesKind.AllVariations;
                    }
                    break;

                case 2:
                    result.Kind = VariationChoices.VariationChoicesKind.Combined;
                    break;
            }

            return result;
        }

        partial void OnFirstTeamValueChanged(int value)
        {
            if (LastTeamValue < value) {
                LastTeamValue = value;
            }
        }

        partial void OnLastTeamValueChanged(int value)
        {
            if (FirstTeamValue > value) {
                FirstTeamValue = value;
            }
        }

        /// <summary>
        /// Clamps a team number to the course relay team range.
        /// </summary>
        /// <param name="team">Team number to clamp.</param>
        /// <returns>A valid team number.</returns>
        private int ClampTeam(int team)
        {
            if (!firstTeam.HasValue || !lastTeam.HasValue) {
                return team;
            }

            if (team < firstTeam.Value) {
                return firstTeam.Value;
            }

            if (team > lastTeam.Value) {
                return lastTeam.Value;
            }

            return team;
        }
    }
}

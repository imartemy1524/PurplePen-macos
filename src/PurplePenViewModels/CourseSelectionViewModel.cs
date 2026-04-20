// CourseSelectionViewModel.cs
//
// Shared ViewModel for selectable course lists in export dialogs.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// Shared course selector used by export dialogs.
    /// </summary>
    public partial class CourseSelectionViewModel : ViewModelBase
    {
        private EventDB? eventDB;
        private bool showVariationChooser;
        private Dictionary<Id<Course>, VariationChoices> variationChoicesPerCourse = new Dictionary<Id<Course>, VariationChoices>();

        /// <summary>
        /// Selectable course rows.
        /// </summary>
        public ObservableCollection<PdfCourseItemViewModel> Courses { get; } = new ObservableCollection<PdfCourseItemViewModel>();

        /// <summary>
        /// Currently selected course row.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsChooseVariationsVisible))]
        [NotifyPropertyChangedFor(nameof(IsChooseVariationsEnabled))]
        private PdfCourseItemViewModel? selectedCourse;

        /// <summary>
        /// Raised when course selection changes.
        /// </summary>
        public event EventHandler? SelectionChanged;

        /// <summary>
        /// True when the choose variations button should be shown.
        /// </summary>
        public bool IsChooseVariationsVisible => showVariationChooser;

        /// <summary>
        /// True when the selected course has variations.
        /// </summary>
        public bool IsChooseVariationsEnabled {
            get {
                return showVariationChooser &&
                       eventDB != null &&
                       SelectedCourse != null &&
                       SelectedCourse.CourseId.IsNotNone &&
                       QueryEvent.HasVariations(eventDB, SelectedCourse.CourseId);
            }
        }

        /// <summary>
        /// Initializes the course selector.
        /// </summary>
        /// <param name="eventDB">The event database.</param>
        /// <param name="selectedCourseIds">Previously selected course ids.</param>
        /// <param name="allCourses">Whether all courses were previously selected.</param>
        /// <param name="variationChoices">Previously selected variation choices.</param>
        /// <param name="showAllControls">Whether the All Controls row should be shown.</param>
        /// <param name="showVariationChooser">Whether variation choosing is enabled.</param>
        public void Initialize(EventDB eventDB, Id<Course>[]? selectedCourseIds, bool allCourses, Dictionary<Id<Course>, VariationChoices>? variationChoices, bool showAllControls, bool showVariationChooser)
        {
            this.eventDB = eventDB;
            this.showVariationChooser = showVariationChooser && QueryEvent.AnyCourseHasVariations(eventDB);
            variationChoicesPerCourse = variationChoices != null
                ? new Dictionary<Id<Course>, VariationChoices>(variationChoices)
                : new Dictionary<Id<Course>, VariationChoices>();

            Courses.Clear();
            List<Id<Course>> selectedIds = selectedCourseIds != null
                ? selectedCourseIds.ToList()
                : QueryEvent.SortedCourseIds(eventDB, true).ToList();
            if (showAllControls && selectedCourseIds == null) {
                selectedIds.Insert(0, Id<Course>.None);
            }

            if (showAllControls) {
                Courses.Add(new PdfCourseItemViewModel(Id<Course>.None, MiscText.AllControls, selectedIds.Contains(Id<Course>.None)));
            }

            foreach (Id<Course> courseId in QueryEvent.SortedCourseIds(eventDB, true)) {
                bool selected = allCourses || selectedIds.Contains(courseId);
                Courses.Add(new PdfCourseItemViewModel(courseId, eventDB.GetCourse(courseId).name, selected));
            }

            foreach (PdfCourseItemViewModel item in Courses) {
                item.PropertyChanged -= CourseItemPropertyChanged;
                item.PropertyChanged += CourseItemPropertyChanged;
            }

            SelectedCourse = Courses.FirstOrDefault(x => x.CourseId.IsNotNone) ?? Courses.FirstOrDefault();
            OnPropertyChanged(nameof(IsChooseVariationsVisible));
            OnPropertyChanged(nameof(IsChooseVariationsEnabled));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Selects every course row.
        /// </summary>
        [RelayCommand]
        private void SelectAllCourses()
        {
            foreach (PdfCourseItemViewModel course in Courses) {
                course.IsSelected = true;
            }
        }

        /// <summary>
        /// Clears every course row.
        /// </summary>
        [RelayCommand]
        private void SelectNoCourses()
        {
            foreach (PdfCourseItemViewModel course in Courses) {
                course.IsSelected = false;
            }
        }

        /// <summary>
        /// Opens the variation selector for the currently selected course.
        /// </summary>
        [RelayCommand]
        private async Task ChooseVariations()
        {
            if (!IsChooseVariationsEnabled || eventDB == null || SelectedCourse == null) {
                return;
            }

            VariationChoices? currentChoices;
            if (!variationChoicesPerCourse.TryGetValue(SelectedCourse.CourseId, out currentChoices)) {
                currentChoices = new VariationChoices();
            }

            SelectVariationsViewModel viewModel = new SelectVariationsViewModel();
            viewModel.Initialize(eventDB, SelectedCourse.CourseId, currentChoices);
            bool result = await Services.DialogService.ShowDialogAsync(viewModel);
            if (result) {
                variationChoicesPerCourse[SelectedCourse.CourseId] = viewModel.BuildVariationChoices();
            }
        }

        /// <summary>
        /// Gets the selected course ids.
        /// </summary>
        /// <returns>The selected course ids.</returns>
        public Id<Course>[] SelectedCourseIds()
        {
            return Courses.Where(x => x.IsSelected).Select(x => x.CourseId).ToArray();
        }

        /// <summary>
        /// Returns true when all actual course rows are selected.
        /// </summary>
        /// <returns>Whether all courses are selected.</returns>
        public bool AllCoursesSelected()
        {
            return Courses.Where(x => x.CourseId.IsNotNone).All(x => x.IsSelected);
        }

        /// <summary>
        /// Gets variation choices for all courses, defaulting missing courses to all variations.
        /// </summary>
        /// <returns>Variation choices keyed by course id.</returns>
        public Dictionary<Id<Course>, VariationChoices> VariationChoicesPerCourse()
        {
            Dictionary<Id<Course>, VariationChoices> result = new Dictionary<Id<Course>, VariationChoices>();
            if (eventDB == null) {
                return result;
            }

            foreach (Id<Course> courseId in QueryEvent.SortedCourseIds(eventDB, true)) {
                VariationChoices? variationChoices;
                if (variationChoicesPerCourse.TryGetValue(courseId, out variationChoices)) {
                    result[courseId] = variationChoices;
                }
                else {
                    result[courseId] = new VariationChoices {
                        Kind = VariationChoices.VariationChoicesKind.AllVariations
                    };
                }
            }

            return result;
        }

        partial void OnSelectedCourseChanged(PdfCourseItemViewModel? value)
        {
            OnPropertyChanged(nameof(IsChooseVariationsEnabled));
        }

        /// <summary>
        /// Handles course row selection changes.
        /// </summary>
        /// <param name="sender">The changed row.</param>
        /// <param name="e">Property change arguments.</param>
        private void CourseItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PdfCourseItemViewModel.IsSelected)) {
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

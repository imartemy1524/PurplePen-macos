// ViewAdditionalCoursesViewModel.cs
//
// ViewModel for selecting additional courses displayed with the active course.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the View Additional Courses dialog.
    /// </summary>
    public partial class ViewAdditionalCoursesViewModel : ViewModelBase
    {
        /// <summary>
        /// Selectable course rows.
        /// </summary>
        public ObservableCollection<PdfCourseItemViewModel> Courses { get; } = new ObservableCollection<PdfCourseItemViewModel>();

        /// <summary>
        /// Initializes the selectable courses list.
        /// </summary>
        /// <param name="eventDB">Current event database.</param>
        /// <param name="currentCourseId">The active course; excluded from the list.</param>
        /// <param name="displayedCourses">Currently displayed extra courses.</param>
        public void Initialize(EventDB eventDB, Id<Course> currentCourseId, List<Id<Course>>? displayedCourses)
        {
            HashSet<Id<Course>> selected = displayedCourses != null
                ? displayedCourses.ToHashSet()
                : new HashSet<Id<Course>>();

            Courses.Clear();
            foreach (Id<Course> courseId in QueryEvent.SortedCourseIds(eventDB, true)) {
                if (courseId == currentCourseId) {
                    continue;
                }

                string name = eventDB.GetCourse(courseId).name;
                Courses.Add(new PdfCourseItemViewModel(courseId, name, selected.Contains(courseId)));
            }
        }

        /// <summary>
        /// Returns selected additional courses.
        /// </summary>
        /// <returns>Selected course identifiers.</returns>
        public List<Id<Course>> GetSelectedCourses()
        {
            return Courses.Where(x => x.IsSelected).Select(x => x.CourseId).ToList();
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (PdfCourseItemViewModel course in Courses) {
                course.IsSelected = true;
            }
        }

        [RelayCommand]
        private void SelectNone()
        {
            foreach (PdfCourseItemViewModel course in Courses) {
                course.IsSelected = false;
            }
        }
    }
}

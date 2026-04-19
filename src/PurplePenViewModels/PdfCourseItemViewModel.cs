// PdfCourseItemViewModel.cs
//
// Represents one selectable course in the PDF export dialog.

using CommunityToolkit.Mvvm.ComponentModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// Represents a selectable course row in the PDF export dialog.
    /// </summary>
    public partial class PdfCourseItemViewModel : ViewModelBase
    {
        /// <summary>
        /// The course identifier.
        /// </summary>
        public Id<Course> CourseId { get; }

        /// <summary>
        /// The display name shown in the dialog.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// True if this course is selected for export.
        /// </summary>
        [ObservableProperty]
        private bool isSelected;

        /// <summary>
        /// Creates a new course item.
        /// </summary>
        /// <param name="courseId">The course identifier.</param>
        /// <param name="name">The display name.</param>
        /// <param name="isSelected">Initial selection state.</param>
        public PdfCourseItemViewModel(Id<Course> courseId, string name, bool isSelected)
        {
            CourseId = courseId;
            Name = name;
            IsSelected = isSelected;
        }

        /// <summary>
        /// Parameterless constructor for the Avalonia designer.
        /// </summary>
        public PdfCourseItemViewModel()
        {
            CourseId = Id<Course>.None;
            Name = "";
        }
    }
}

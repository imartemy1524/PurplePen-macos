// ChangeCourseOrderDialogViewModel.cs
//
// ViewModel for the Change Course Order dialog. Presents the list of
// courses in the current sort order and lets the user move items up/down.
//
// Migrated from WinForms PurplePen/ChangeCourseOrder.cs.

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// Row item shown in the Change Course Order dialog.
    /// </summary>
    public partial class CourseOrderRowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string courseName = "";

        [ObservableProperty]
        private int sourceIndex;
    }

    /// <summary>
    /// ViewModel for the Change Course Order dialog.
    /// </summary>
    public partial class ChangeCourseOrderDialogViewModel : ViewModelBase
    {
        private Controller.CourseOrderInfo[] courseOrders = Array.Empty<Controller.CourseOrderInfo>();

        /// <summary>
        /// The displayed course rows.
        /// </summary>
        public ObservableCollection<CourseOrderRowViewModel> Rows { get; } = new ObservableCollection<CourseOrderRowViewModel>();

        /// <summary>
        /// Initializes the dialog from controller data.
        /// </summary>
        public void SetCourseOrders(Controller.CourseOrderInfo[] orders)
        {
            courseOrders = orders;
            Array.Sort(courseOrders, delegate (Controller.CourseOrderInfo order1, Controller.CourseOrderInfo order2) {
                return order1.sortOrder.CompareTo(order2.sortOrder);
            });

            Rows.Clear();
            for (int i = 0; i < courseOrders.Length; ++i) {
                Rows.Add(new CourseOrderRowViewModel {
                    CourseName = courseOrders[i].courseName,
                    SourceIndex = i
                });
            }
        }

        /// <summary>
        /// Moves the row at the given index up one position.
        /// </summary>
        public void MoveUp(int index)
        {
            if (index > 0 && index < Rows.Count) {
                Rows.Move(index, index - 1);
            }
        }

        /// <summary>
        /// Moves the row at the given index down one position.
        /// </summary>
        public void MoveDown(int index)
        {
            if (index >= 0 && index < Rows.Count - 1) {
                Rows.Move(index, index + 1);
            }
        }

        /// <summary>
        /// Applies the current UI ordering back into the controller array.
        /// </summary>
        public Controller.CourseOrderInfo[] GetCourseOrders()
        {
            for (int i = 0; i < Rows.Count; ++i) {
                courseOrders[Rows[i].SourceIndex].sortOrder = i + 1;
            }

            return courseOrders;
        }
    }
}

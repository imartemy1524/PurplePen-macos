// ViewAdditionalCourses.axaml.cs
//
// Code-behind for selecting additional displayed courses.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog used to choose additional courses displayed with the active course.
    /// </summary>
    public partial class ViewAdditionalCourses : Window
    {
        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public ViewAdditionalCourses()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}

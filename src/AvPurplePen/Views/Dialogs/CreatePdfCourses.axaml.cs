// CreatePdfCourses.axaml.cs
//
// Code-behind for the course PDF export dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for selecting courses and output settings for course PDF export.
    /// </summary>
    public partial class CreatePdfCourses : Window
    {
        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        public CreatePdfCourses()
        {
            InitializeComponent();
            Opened += (_, _) => courseListBox.Focus();
        }

        /// <summary>
        /// Accepts the dialog.
        /// </summary>
        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        /// <summary>
        /// Cancels the dialog.
        /// </summary>
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}

// ChangeCourseOrderDialog.axaml.cs
//
// Code-behind for the Change Course Order dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for reordering courses.
    /// </summary>
    public partial class ChangeCourseOrderDialog : Window
    {
        public ChangeCourseOrderDialog()
        {
            InitializeComponent();
            Opened += (s, e) => {
                listBoxCourses.Focus();
                UpdateButtonState();
            };
        }

        private void MoveUpButton_Click(object? sender, RoutedEventArgs e)
        {
            ChangeCourseOrderDialogViewModel? vm = DataContext as ChangeCourseOrderDialogViewModel;
            if (vm == null)
                return;

            int index = listBoxCourses.SelectedIndex;
            vm.MoveUp(index);
            if (index > 0) {
                listBoxCourses.SelectedIndex = index - 1;
            }
            UpdateButtonState();
        }

        private void MoveDownButton_Click(object? sender, RoutedEventArgs e)
        {
            ChangeCourseOrderDialogViewModel? vm = DataContext as ChangeCourseOrderDialogViewModel;
            if (vm == null)
                return;

            int index = listBoxCourses.SelectedIndex;
            vm.MoveDown(index);
            if (index >= 0 && index < vm.Rows.Count - 1) {
                listBoxCourses.SelectedIndex = index + 1;
            }
            UpdateButtonState();
        }

        private void ListBoxCourses_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            ChangeCourseOrderDialogViewModel? vm = DataContext as ChangeCourseOrderDialogViewModel;
            if (vm == null)
                return;

            int index = listBoxCourses.SelectedIndex;
            moveUpButton.IsEnabled = (index > 0);
            moveDownButton.IsEnabled = (index >= 0 && index < vm.Rows.Count - 1);
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

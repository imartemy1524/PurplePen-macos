// NewEventWizard.axaml.cs
//
// Code-behind for the New Event wizard. The DataContext
// (NewEventWizardViewModel) is set by the caller before showing the dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Wizard dialog that gathers the information needed to create a new event.
    /// </summary>
    public partial class NewEventWizard : Window
    {
        /// <summary>
        /// Initializes the wizard and its components.
        /// </summary>
        public NewEventWizard()
        {
            InitializeComponent();
            Opened += (sender, args) => titleText.Focus();
        }

        /// <summary>
        /// Moves to the previous wizard page.
        /// </summary>
        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is NewEventWizardViewModel viewModel) {
                viewModel.Back();
            }
        }

        /// <summary>
        /// Moves to the next page or finishes the wizard.
        /// </summary>
        private async void NextButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is NewEventWizardViewModel viewModel) {
                bool accepted = await viewModel.NextOrFinishAsync();
                if (accepted) {
                    Close(true);
                }
            }
        }

        /// <summary>
        /// Cancels the wizard.
        /// </summary>
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}

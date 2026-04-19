// LegAssignmentsDialogViewModel.cs
//
// ViewModel for the relay fixed-branch assignments dialog.
// Keeps one row per branch code and lets the user assign legs by typing
// 1-based leg numbers into a text field.
//
// Migrated from WinForms PurplePen/LegAssignmentsDialog.cs.

using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// One editable row in the fixed-branch assignments dialog.
    /// </summary>
    public partial class LegAssignmentRowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private char branchCode;

        [ObservableProperty]
        private string legsText = "";
    }

    /// <summary>
    /// ViewModel for the fixed-branch assignments dialog.
    /// </summary>
    public partial class LegAssignmentsDialogViewModel : ViewModelBase
    {
        private Controller? controller;
        private int numberOfLegs;

        /// <summary>
        /// Rows shown in the dialog.
        /// </summary>
        public ObservableCollection<LegAssignmentRowViewModel> Rows { get; } = new ObservableCollection<LegAssignmentRowViewModel>();

        /// <summary>
        /// Initializes the dialog rows from the possible branch codes and the
        /// current assignments.
        /// </summary>
        public void Initialize(Controller controller, List<char[]> codes, FixedBranchAssignments assignments, int numberOfLegs)
        {
            this.controller = controller;
            this.numberOfLegs = numberOfLegs;

            Rows.Clear();

            for (int groupIndex = 0; groupIndex < codes.Count; ++groupIndex) {
                char[] branchGroup = codes[groupIndex];
                foreach (char code in branchGroup) {
                    LegAssignmentRowViewModel row = new LegAssignmentRowViewModel {
                        BranchCode = code,
                        LegsText = assignments.BranchIsFixed(code) ? CreateLegText(assignments.FixedLegsForBranch(code)) : ""
                    };
                    Rows.Add(row);
                }
            }
        }

        /// <summary>
        /// Gets the assignments represented by the current rows.
        /// </summary>
        public FixedBranchAssignments FixedBranchAssignments
        {
            get {
                FixedBranchAssignments fixedBranchAssignments = new FixedBranchAssignments();

                foreach (LegAssignmentRowViewModel row in Rows) {
                    List<int> legs = ParseLegText(row.LegsText);
                    foreach (int leg in legs) {
                        fixedBranchAssignments.AddBranchAssignment(row.BranchCode, leg);
                    }
                }

                return fixedBranchAssignments;
            }
        }

        /// <summary>
        /// Validates the current assignments against the current course.
        /// </summary>
        public bool TryValidate(out string errorMessage)
        {
            if (controller == null) {
                errorMessage = "";
                return true;
            }

            FixedBranchAssignments assignments = FixedBranchAssignments;
            errorMessage = controller.ValidateFixedBranchAssignments(numberOfLegs, assignments) ?? "";
            return errorMessage.Length == 0;
        }

        private static string CreateLegText(ICollection<int> legs)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            foreach (int leg in legs) {
                if (builder.Length != 0)
                    builder.Append(", ");
                builder.Append((leg + 1).ToString());
            }

            return builder.ToString();
        }

        private static List<int> ParseLegText(string legText)
        {
            List<int> result = new List<int>();

            if (string.IsNullOrWhiteSpace(legText))
                return result;

            string[] fields = legText.Split(new[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in fields) {
                int leg;
                if (int.TryParse(s, out leg))
                    result.Add(leg - 1);
            }
            return result;
        }
    }
}

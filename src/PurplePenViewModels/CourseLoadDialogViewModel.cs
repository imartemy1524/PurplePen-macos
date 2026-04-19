// CourseLoadDialogViewModel.cs
//
// ViewModel for the Course Load dialog. Keeps the course rows in the same
// order as the controller returns them and allows editing the load field.
//
// Migrated from WinForms PurplePen/CourseLoad.cs.

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// Row item shown in the Course Load dialog.
    /// </summary>
    public partial class CourseLoadRowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string courseName = "";

        [ObservableProperty]
        private string loadText = "";

        [ObservableProperty]
        private int sourceIndex;
    }

    /// <summary>
    /// ViewModel for the Course Load dialog.
    /// </summary>
    public partial class CourseLoadDialogViewModel : ViewModelBase
    {
        private Controller.CourseLoadInfo[] courseLoads = Array.Empty<Controller.CourseLoadInfo>();

        /// <summary>
        /// The displayed course rows.
        /// </summary>
        public ObservableCollection<CourseLoadRowViewModel> Rows { get; } = new ObservableCollection<CourseLoadRowViewModel>();

        /// <summary>
        /// Initializes the rows from the controller data.
        /// </summary>
        public void SetCourseLoads(Controller.CourseLoadInfo[] loads)
        {
            courseLoads = loads;
            Rows.Clear();

            for (int i = 0; i < loads.Length; ++i) {
                string loadString = loads[i].load < 0 ? "" : loads[i].load.ToString();
                Rows.Add(new CourseLoadRowViewModel {
                    CourseName = loads[i].courseName,
                    LoadText = loadString,
                    SourceIndex = i
                });
            }
        }

        /// <summary>
        /// Commits the edited load values back to the controller array.
        /// </summary>
        public bool TryCommitLoads(out string errorMessage)
        {
            int[] parsedLoads = new int[Rows.Count];

            for (int i = 0; i < Rows.Count; ++i) {
                CourseLoadRowViewModel row = Rows[i];
                if (!TryParseLoad(row.LoadText, out int load)) {
                    errorMessage = MiscText.BadLoad;
                    return false;
                }

                parsedLoads[i] = load;
            }

            foreach (CourseLoadRowViewModel row in Rows) {
                int load = parsedLoads[row.SourceIndex];
                courseLoads[row.SourceIndex].load = load;
            }

            errorMessage = "";
            return true;
        }

        /// <summary>
        /// Gets the course loads after committing the edited values.
        /// </summary>
        public Controller.CourseLoadInfo[] GetCourseLoads()
        {
            return courseLoads;
        }

        /// <summary>
        /// Parses a load value. Blank means "no load" (-1).
        /// </summary>
        public static bool TryParseLoad(string? loadString, out int load)
        {
            if (loadString == null)
                loadString = "";

            string trimmed = loadString.Trim();
            if (trimmed == "") {
                load = -1;
                return true;
            }

            return int.TryParse(trimmed, out load);
        }
    }
}

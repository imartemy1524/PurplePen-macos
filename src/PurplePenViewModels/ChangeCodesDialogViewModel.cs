using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PurplePen;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Change Codes dialog.
    /// Allows editing of control point codes.
    /// </summary>
    public partial class ChangeCodesDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<CodeEntry> codes = new();

        [ObservableProperty]
        private bool dialogResult = false;

        public partial class CodeEntry : ObservableObject
        {
            [ObservableProperty]
            private int controlNumber;

            [ObservableProperty]
            private string code = "";

            public CodeEntry()
            {
            }

            public CodeEntry(int number, string codeValue)
            {
                ControlNumber = number;
                Code = codeValue;
            }
        }

        public ChangeCodesDialogViewModel()
        {
        }

        public void Initialize(KeyValuePair<object, string>[] allCodes)
        {
            Codes.Clear();
            foreach (var kvp in allCodes)
            {
                // Extract the int id from the Id<T> object
                int controlNumber = 0;
                if (kvp.Key is Id<ControlPoint> controlId)
                {
                    controlNumber = controlId.id;
                }
                else if (kvp.Key is int intId)
                {
                    controlNumber = intId;
                }

                Codes.Add(new CodeEntry(controlNumber, kvp.Value));
            }
        }

        public KeyValuePair<object, string>[] GetUpdatedCodes()
        {
            return Codes.Select(c => new KeyValuePair<object, string>((object)new Id<ControlPoint>(c.ControlNumber), c.Code)).ToArray();
        }

        [RelayCommand]
        private void Ok()
        {
            DialogResult = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false;
        }
    }
}


using DigitizingNoteFs.Core.Models;

namespace DigitizingNoteFs.Wpf.ViewModels
{
    internal partial class MainWindowViewModel : BaseViewModel
    {
        private string? _selectedItem;
        public string? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        private FsSheetModel? _fsSheet;
        public FsSheetModel? SheetModel
        {
            get => _fsSheet;
            set
            {
                _fsSheet = value ?? new FsSheetModel();
                OnPropertyChanged(nameof(SheetModel));
            }
        }

        public MainWindowViewModel()
        {
        }
        
    }

    internal partial class MainWindowViewModel
    {
       
    }
}

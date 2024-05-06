using CommunityToolkit.Mvvm.ComponentModel;
using DigitizingNoteFs.Core.Common;
using System.Collections.ObjectModel;


namespace DigitizingNoteFs.Core.ViewModels
{
    public partial class MoneyMappingViewModel : ObservableObject
    {
        public ObservableCollection<ComboBoxPairs> NoteFsChildren { get; set; }

        [ObservableProperty]
        private ComboBoxPairs? selectedNoteFsChild;

        [ObservableProperty]
        private double value = 0.0;

        [ObservableProperty]
        private int noteId = 0;

        [ObservableProperty]
        private int selectedIndex;

        public MoneyMappingViewModel()
        {
            NoteFsChildren = [];
        }
    }

    public partial class MoneyMappingViewModel
    {
        #region SelectedNoteFsChild Event

        partial void OnSelectedNoteFsChildChanging(ComboBoxPairs? oldValue, ComboBoxPairs? newValue)
        {
            int newNoteId;

            int.TryParse(newValue?.Key, out newNoteId);
            if (newNoteId != NoteId)
            {
                NoteId = newNoteId;
            }
        }

        #endregion



    }
}

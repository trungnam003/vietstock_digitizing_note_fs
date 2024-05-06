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
            if(oldValue == null)
            {
                return;
            }
            SelectedNoteFsChildChanging?.Invoke(this, EventArgs.Empty);

        }

        partial void OnSelectedNoteFsChildChanged(ComboBoxPairs? oldValue, ComboBoxPairs? newValue)
        {
            if (oldValue == null)
            {
                return;
            }
            SelectedNoteFsChildChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public event EventHandler? SelectedNoteFsChildChanged;
        public event EventHandler? SelectedNoteFsChildChanging;
    }


}

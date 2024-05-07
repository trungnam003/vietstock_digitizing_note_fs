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
            var args = new MoneyMappingEventArgs(oldValue, newValue);
            SelectedNoteFsChildChanging?.Invoke(this, args);
        }

        partial void OnSelectedNoteFsChildChanged(ComboBoxPairs? oldValue, ComboBoxPairs? newValue)
        {
            if (newValue != null)
            {
                NoteId = int.TryParse(newValue.Key, out var id) ? id : 0;
            }
            var args = new MoneyMappingEventArgs(oldValue, newValue);
            SelectedNoteFsChildChanged?.Invoke(this, args);
        }

        #endregion

        public event EventHandler? SelectedNoteFsChildChanged;
        public event EventHandler? SelectedNoteFsChildChanging;
    }

    public class MoneyMappingEventArgs : EventArgs
    {
        public MoneyMappingEventArgs(object? oldValue, object? newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public object? OldValue { get; }
        public object? NewValue { get; }
    }

}

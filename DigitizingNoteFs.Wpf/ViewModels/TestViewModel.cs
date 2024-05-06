using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitizingNoteFs.Core.Common;
using DigitizingNoteFs.Core.ViewModels;
using System.Collections.ObjectModel;

namespace DigitizingNoteFs.Wpf.ViewModels
{
    public partial class TestViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<MoneyMappingViewModel> moneyMappingData = [];

        partial void OnMoneyMappingDataChanged(ObservableCollection<MoneyMappingViewModel>? oldValue, ObservableCollection<MoneyMappingViewModel> newValue)
        {

        }

        public IRelayCommand TestCommand { get; }

        public TestViewModel()
        {
            TestCommand = new RelayCommand(OnTestCommand);
        }

        private void OnTestCommand()
        {
            var lst = new List<ComboBoxPairs>();
            lst.Add(new ComboBoxPairs("1", "1"));
            lst.Add(new ComboBoxPairs("2", "2"));
            lst.Add(new ComboBoxPairs("3", "3"));
            lst.Add(new ComboBoxPairs("4", "4"));

            MoneyMappingData.Clear();

            var model = new MoneyMappingViewModel();
            model.Value = 100;
            model.NoteId = 10;
            model.NoteFsChildren = new(lst.Select(x => new ComboBoxPairs(x.Key, x.Value)).ToList());
            model.SelectedNoteFsChild = model.NoteFsChildren.FirstOrDefault();
            MoneyMappingData.Add(model);

            model = new MoneyMappingViewModel();
            model.Value = 200;
            model.NoteId = 20;
            model.NoteFsChildren = new(lst.Select(x => new ComboBoxPairs(x.Key, x.Value)).ToList());
            model.SelectedNoteFsChild = model.NoteFsChildren.FirstOrDefault();

            MoneyMappingData.Add(model);

            //Task.Run(async () =>
            //{
            //    // sleep for 2 seconds
            //    await Task.Delay(100);
            //    foreach (var item in moneyMappingData)
            //    {
            //        // random SelectedNoteFsChild
            //        item.SelectedNoteFsChild = item.NoteFsChildren[new Random().Next(0, item.NoteFsChildren.Count)];

            //    }
            //});

        }
    }
}

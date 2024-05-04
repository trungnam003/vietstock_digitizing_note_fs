using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DigitizingNoteFs.Wpf.ViewModels
{
    public partial class TestViewModel : ObservableObject
    {
        private ObservableCollection<PersonViewModel> _people = []; 
        public ObservableCollection<PersonViewModel> People
        {
            get => _people;
            set => SetProperty(ref _people, value);
        }

        public IRelayCommand UpdateOnePersonCommand { get; set; }


        public TestViewModel()
        {
            People = new ObservableCollection<PersonViewModel>(Enumerable.Range(0, 100).Select(i => new PersonViewModel { Name = $"Person {i}", Age = 20 + i }).ToList());
            UpdateOnePersonCommand = new RelayCommand(() =>
            {
                MessageBox.Show("Update one person");
            });
            // run background task to update person
            _ = RandomUpdatePerson();
        }

        private async Task RandomUpdatePerson()
        {
            while(true)
            {
                await Task.Delay(5);
                var random = new Random();
                var index = random.Next(0, People.Count);
                var person = People[index];
                person.Age = random.Next(20, 60);
                person.IsModified = person.Age % 2 == 0;    
            }
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;


namespace DigitizingNoteFs.Wpf.ViewModels
{
    public partial class PersonViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private int age;

        [ObservableProperty]
        private bool isModified;
    }
}

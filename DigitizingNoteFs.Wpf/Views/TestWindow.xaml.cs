using DigitizingNoteFs.Wpf.ViewModels;
using System.Windows;

namespace DigitizingNoteFs.Wpf.Views
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
            DataContext = new TestViewModel();
        }
    }
}

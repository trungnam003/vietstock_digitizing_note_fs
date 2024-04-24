using DigitizingNoteFs.Core.Models;
using DigitizingNoteFs.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;


namespace DigitizingNoteFs.Wpf
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = InitDataContext();
            this.DataContext = _viewModel;
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                var note = row.Item as FsNoteModel;
            }
        }
    }

    public partial class MainWindow
    {
        private MainWindowViewModel InitDataContext()
        {
            return new MainWindowViewModel();
        }
    }
}
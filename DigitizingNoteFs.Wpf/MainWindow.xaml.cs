
using DigitizingNoteFs.Core.Common;
using DigitizingNoteFs.Core.Services;
using DigitizingNoteFs.Wpf.ViewModels;
using Microsoft.Win32;
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

        private void cbbFsSheets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = cbbFsSheets.SelectedValue;
            
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
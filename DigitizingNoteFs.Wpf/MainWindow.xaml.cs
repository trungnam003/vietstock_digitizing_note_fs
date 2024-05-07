using DigitizingNoteFs.Core.ViewModels;
using DigitizingNoteFs.Wpf.ViewModels;
using Force.DeepCloner;
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
                if (row.Item is not FsNoteParentViewModel note)
                {
                    return;
                }
                _viewModel.ChangeSuggestFsNoteParent(note);
               
            }
        }

        private void dgDataFsSheet_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridCell cell)
            {
                cell.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // copy cell value
            if (sender is MenuItem menuItem)
            {
                if (menuItem.DataContext is DataGridCell cell)
                {
                    Clipboard.SetText(cell.Content.ToString());
                }
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
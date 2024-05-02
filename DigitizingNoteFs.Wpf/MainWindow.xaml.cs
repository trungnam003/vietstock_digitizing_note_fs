using DigitizingNoteFs.Core.Models;
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
                if (row.Item is not FsNoteModel note)
                {
                    return;
                }

                var children = _viewModel.Data.Where(x => x.ParentId == note.FsNoteId && x.Group == note.Group).Select(x => x.DeepClone()).ToList();
                _viewModel.SuggestedFsNoteChildren = new(children);
                _viewModel.SuggestedFsNoteParent = note;
            }
        }

        private void dgDataFsSheet_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell != null)
            {
                cell.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // copy cell value
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var cell = menuItem.DataContext as DataGridCell;
                if (cell != null)
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
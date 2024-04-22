
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
        public ExcelService _excelService { get; set; }

        public MainWindow(ExcelService excelService)
        {
            this._excelService = excelService;
            InitializeComponent();
            _viewModel = InitDataContext();
            this.DataContext = _viewModel;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Chọn file import thuyết minh",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "xls",
                Filter = "xls files (*.xls)|*.xls",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                FilePathTextBox.Text = selectedFileName;
                BindingCombobox();
                var selected = cbbFsSheets.SelectedValue;
                var fsModel = _excelService.ReadImportFsExcelFile(selectedFileName, sheetName: selected.ToString() ?? throw new ArgumentNullException());
                _viewModel.SheetModel = fsModel;

            }
        }

        private void cbbFsSheets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = cbbFsSheets.SelectedValue;
            var fsModel = _excelService.ReadImportFsExcelFile(FilePathTextBox.Text, sheetName: selected.ToString() ?? throw new ArgumentNullException());
            _viewModel.SheetModel = fsModel;
        }
    }

    public partial class MainWindow
    {
        private void BindingCombobox()
        {
            string[]? fsSheets = this.FindResource("FsSheets") as string[]
                ?? throw new Exception("FsSheets not found in Resource XAML");
            List<ComboBoxPairs> comboBoxPairs = [];
            foreach (var fsSheet in fsSheets)
            {
                comboBoxPairs.Add(new ComboBoxPairs(fsSheet, fsSheet));
            }
            cbbFsSheets.ItemsSource = comboBoxPairs;
            cbbFsSheets.DisplayMemberPath = "Key";
            cbbFsSheets.SelectedValuePath = "Value";
            cbbFsSheets.SelectedIndex = 0;
        }
        private MainWindowViewModel InitDataContext()
        {
            return new MainWindowViewModel();
        }
    }
}
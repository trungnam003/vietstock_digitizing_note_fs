
using DigitizingNoteFs.Core.Common;
using DigitizingNoteFs.Core.Models;
using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DigitizingNoteFs.Wpf.ViewModels
{
    internal partial class MainWindowViewModel : BaseViewModel
    {

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
            }
        }
        private FsSheetModel? _fsSheet;
        public FsSheetModel? SheetModel
        {
            get => _fsSheet;
            set
            {
                _fsSheet = value ?? new FsSheetModel();
                OnPropertyChanged(nameof(SheetModel));
            }
        }

        private ObservableCollection<ComboBoxPairs> _sheetNames;
        public ObservableCollection<ComboBoxPairs> SheetNames { get; set; }
        

        private ObservableCollection<string> _data;
        public ObservableCollection<string> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }

        private ComboBoxPairs _selectedSheet;
        public ComboBoxPairs SelectedSheet
        {
            get { return _selectedSheet; }
            set
            {

                _selectedSheet = value;
                LoadDataFromSheet(_selectedSheet.Value);
                OnPropertyChanged("SelectedSheet");
            }
        }

        private HSSFWorkbook? _workbook = null;

        private ICommand _openFileCommand;
        public ICommand OpenFileCommand
        {
            get
            {
                return _openFileCommand ?? (_openFileCommand = new RelayCommand(x => OpenFile()));
            }
        }

        public MainWindowViewModel()
        {
            SheetNames = new ObservableCollection<ComboBoxPairs>();
            Data = new ObservableCollection<string>();
        }

        private void LoadDataFromSheet(string sheetName)
        {
            if (string.IsNullOrEmpty(sheetName))
                return;

            Data.Clear();

            if (_workbook != null)
            {
                var sheet = _workbook.GetSheet(sheetName);
                if (sheet != null)
                {
                    for (int i = 0; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        if (row != null)
                        {
                            for (int j = 0; j < row.LastCellNum; j++)
                            {
                                var cell = row.GetCell(j);
                                if (cell != null)
                                {
                                    Data.Add(cell.ToString());
                                }
                            }
                        }
                    }
                }
            }

        }
        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx;*.xlsm"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;

                using FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                _workbook = new HSSFWorkbook(file);
                LoadSheetNames();
                file.Close();
            }
        }

        private void LoadSheetNames()
        {
            SheetNames.Clear();

            try
            {
                if(_workbook != null)
                {
                    for (int i = 0; i < _workbook.NumberOfSheets; i++)
                    {
                        var sheet = _workbook.GetSheetAt(i);
                        if (sheet.SheetName.StartsWith("TM"))
                        {
                            SheetNames.Add(new(sheet.SheetName, sheet.SheetName));
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }

    internal partial class MainWindowViewModel
    {
       
    }
}

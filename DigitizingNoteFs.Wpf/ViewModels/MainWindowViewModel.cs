﻿
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitizingNoteFs.Core.Common;
using DigitizingNoteFs.Core.Models;
using DigitizingNoteFs.Shared.Utilities;
using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace DigitizingNoteFs.Wpf.ViewModels
{
    internal partial class MainWindowViewModel : ObservableObject
    {
        #region Observable Properties
        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                SetProperty(ref _filePath, value);
            }
        }

        private FsSheetModel? _fsSheet;
        public FsSheetModel? SheetModel
        {
            get => _fsSheet;
            set
            {
                SetProperty(ref _fsSheet, value);
            }
        }

        private ObservableCollection<FsNoteModel> _data;
        public ObservableCollection<FsNoteModel> Data
        {
            get { return _data; }
            set
            {
                SetProperty(ref _data, value);
            }
        }

        private ComboBoxPairs _selectedSheet;
        public ComboBoxPairs SelectedSheet
        {
            get { return _selectedSheet; }
            set
            {
                SetProperty(ref _selectedSheet, value);
                LoadDataFromSheet(_selectedSheet.Value);
            }
        }

        private ObservableGroupedCollection<int, FsNoteModel> _groupedData;
        public ObservableGroupedCollection<int, FsNoteModel> GroupedData
        {
            get { return _groupedData; }
            set
            {
                SetProperty(ref _groupedData, value);
            }
        }

        private ObservableCollection<FsNoteModel> _parentNoteData;
        public ObservableCollection<FsNoteModel> ParentNoteData
        {
            get { return _parentNoteData; }
            set
            {
                SetProperty(ref _parentNoteData, value);
            }
        }

        private string _inputText;
        public string InputText
        {
            get { return _inputText; }
            set
            {
                _inputText = value;
                OnPropertyChanged("InputText");
                StartDebounceTimer();
            }
        }
        #endregion

        #region Properties

        public List<ComboBoxPairs> SheetNames { get; set; }
        public Dictionary<int, List<FsNoteMappingModel>> Mapping { get; set; } = [];
        private HSSFWorkbook? _workbook = null;
        private Timer debounceTimer;
        #endregion

        #region Commands
        public IRelayCommand OpenFileCommand { get; }
        public IRelayCommand PasteTextCommand { get; }
        public IRelayCommand ClearTextCommand { get; }
        public IRelayCommand OpenAbbyyScreenShotCommand { get; }
        #endregion

        #region Constructors
        public MainWindowViewModel()
        {
            SheetNames = new List<ComboBoxPairs>();
            Data = new ObservableCollection<FsNoteModel>();
            OpenFileCommand = new RelayCommand(OpenFile);
            PasteTextCommand = new RelayCommand(GetDataFromClipboard);
            ClearTextCommand = new RelayCommand(ClearText);
            OpenAbbyyScreenShotCommand = new RelayCommand<object>(OpenAbbyyScreenShot);
            GroupedData = new();
            ParentNoteData = new();
            debounceTimer = new Timer(DebounceTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }
        #endregion

        #region Methods

        private void OpenAbbyyScreenShot(object? obj)
        {
            var abbyyPath = @"D:\Abbyy\Abbyy15\ABBYY FineReader 15\ScreenshotReader.exe";
            if (File.Exists(abbyyPath))
            {
                Process.Start(abbyyPath);
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }
            else
            {
                MessageBox.Show("Không tìm thấy phần mềm ABBYY FineReader 12");
            }
        }

        private void ClearText()
        {
            InputText = string.Empty;
        }

        private void GetDataFromClipboard()
        {
            var clipboardText = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardText))
            {
                return;
            }
            InputText = clipboardText;
        }

        private void LoadDataFromSheet(string sheetName)
        {
            if (string.IsNullOrEmpty(sheetName))
                return;

            Data.Clear();
            ParentNoteData.Clear();

            if (_workbook == null)
            {
                return;
            }
            var sheet = _workbook.GetSheet(sheetName);
            if (sheet == null)
            {
                return;
            }
            const int COL_NOTE_ID = 3;
            const int COL_CHECK_PARENT_NOTE = 2;
            const int COL_NOTE_NAME = 4;
            const int COL_NOTE_PARENT_VALUE = 6;
            const int START_ROW_INDEX = 14;
            const int COL_NOTE_VALUE = 5;

            int currentParentId = 0;

            for (int i = START_ROW_INDEX; i <= sheet.LastRowNum; i++)
            {
                try
                {
                    var row = sheet.GetRow(i);
                    if (row == null)
                    {
                        continue;
                    }
                    var model = new FsNoteModel();
                    string? name = row.GetCell(COL_NOTE_NAME).ToString();
                    bool inValidNoteId = !int.TryParse(row.GetCell(COL_NOTE_ID).ToString(), out int noteId) && noteId == 0;
                    bool inValidName = string.IsNullOrEmpty(name);
                    if (inValidNoteId && inValidName)
                    {
                        continue;
                    }
                    model.FsNoteId = noteId;
                    model.Name = name;

                    var cellCheckParent = row.GetCell(COL_CHECK_PARENT_NOTE);

                    if (cellCheckParent == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(cellCheckParent.ToString()))
                    {
                        var cellValue = row.GetCell(COL_NOTE_VALUE);
                        if (cellValue.CellType == CellType.Formula)
                        {
                            continue;
                        }
                        model.IsParent = false;
                        model.ParentId = currentParentId;
                        model.Cell = new(i, COL_NOTE_VALUE);
                        model.CellAddress = $"{(char)('A' + COL_NOTE_VALUE)}{i}";
                    }
                    else
                    {
                        if (!Mapping.ContainsKey(noteId))
                        {
                            continue;
                        }
                        model.IsParent = true;
                        model.ParentId = 0;
                        currentParentId = noteId;

                        var cellParentValue = row.GetCell(COL_NOTE_PARENT_VALUE);
                        if (cellParentValue == null)
                        {
                            model.Value = 0;
                        }
                        else
                        {
                            model.Value = cellParentValue.NumericCellValue;
                        }
                        ParentNoteData.Add(model);
                    }

                    Data.Add(model);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            var d = Data;
            //GroupedData.Clear();
            //GroupedData = new(Data.Where(x => x.ParentId != 0).GroupBy(x => x.ParentId));
        }
        private void OpenFile()
        {
            try
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
                    FilePath = openFileDialog.FileName;

                    using var file = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                    _workbook = new HSSFWorkbook(file);
                    LoadSheetNames();
                    LoadMappingData();
                    file.Close();
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadSheetNames()
        {
            SheetNames.Clear();
            try
            {
                if (_workbook != null)
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

        private void LoadMappingData()
        {
            Mapping.Clear();
            try
            {
                if (_workbook == null)
                {
                    return;
                }

                var sheet = _workbook.GetSheet("Mapping");
                const int START_ROW = 1;
                const int COL_NOTE_ID = 0;
                const int COL_KEYWORD = 2;
                const int COL_IS_PARENT = 3;
                int currentParentId = 0;
                for (int i = START_ROW; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null)
                    {
                        continue;
                    }
                    var cellNoteId = row.GetCell(COL_NOTE_ID);
                    var cellKeyword = row.GetCell(COL_KEYWORD);
                    var cellIsParent = row.GetCell(COL_IS_PARENT);

                    if (cellNoteId == null)
                    {
                        continue;
                    }
                    var model = new FsNoteMappingModel();

                    bool isParent = cellIsParent != null && cellIsParent.StringCellValue.ToLower().Equals("x");
                    if (isParent)
                    {
                        currentParentId = (int)cellNoteId.NumericCellValue;
                        if (!Mapping.ContainsKey(currentParentId))
                        {
                            Mapping[currentParentId] = [];
                        }
                        continue;
                    }

                    model.Id = (int)cellNoteId.NumericCellValue;
                    if (model.Id == 0)
                    {
                        continue;
                    }
                    var rawKeyword = cellKeyword == null ? string.Empty : cellKeyword.ToString();
                    if (!string.IsNullOrEmpty(rawKeyword))
                    {
                        var keywords = new List<string>(rawKeyword.Split(','));
                        model.Keywords = keywords.Select(x => x.Trim()).ToList();
                    }
                    model.ParentId = currentParentId;
                    Mapping[currentParentId].Add(model);
                }
                var d = Mapping;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void StartDebounceTimer()
        {
            debounceTimer.Change(500, Timeout.Infinite);
        }

        private void DebounceTimerCallback(object? state)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                var result = StringUtils.ConvertToMatrix(InputText);
                HandleData(result);
            });

        }

        private void HandleData(List<List<string>> data)
        {
            var moneyList = new List<MoneyCell>();
            var sum = 0L;
            var max = long.MinValue;
            int iRow = 0, iCol = 0;   
            data.ForEach(row =>
            {
                row.ForEach(cell =>
                {
                    var text = cell;
                    var regexMoney = StringUtils.MoneyStringPattern;
                    var matches = Regex.Matches(text, regexMoney);
                    foreach (Match match in matches.Cast<Match>())
                    {
                        var rawMoney = match.Value.Replace(",", string.Empty).Replace(".", string.Empty);

                        if (rawMoney.StartsWith("(") && rawMoney.EndsWith(")"))
                        {
                            rawMoney = string.Concat("-", rawMoney.AsSpan(1, rawMoney.Length - 2));
                        }

                        var money = long.Parse(rawMoney);

                        if (money < 1000 && money > -1000)
                        {
                            continue;
                        }
                        var moneyModel = new MoneyCell
                        {
                            Row = iRow,
                            Col = iCol,
                            Value = money
                        };
                        moneyList.Add(moneyModel);
                        sum += money;
                        max = Math.Max(max, money);
                    }
                    iCol++;
                });
                iRow++;
                iCol = 0;
            });
            SuggestParentNote(sum, max);
        }

        private void SuggestParentNote(long sum, long max)
        {
            // Nếu đúng thì người dùng có quét tổng, nếu sai thì không quét tổng
            var debug = sum - max == max;
            var parentNote = ParentNoteData.FirstOrDefault(x => x.Value == max);
        }
        #endregion
    }

}

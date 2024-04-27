
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitizingNoteFs.Core.Common;
using DigitizingNoteFs.Core.Models;
using DigitizingNoteFs.Shared.Utilities;
using DigitizingNoteFs.Wpf.Services;
using Force.DeepCloner;
using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Match = System.Text.RegularExpressions.Match;

namespace DigitizingNoteFs.Wpf.ViewModels
{
    internal partial class MainWindowViewModel : ObservableObject
    {
        #region Observable Properties
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                SetProperty(ref _isLoading, value);
            }
        }
        private string _filePath;
        /// <summary>
        /// Đường dẫn file excel TMBCTC
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                SetProperty(ref _filePath, value);
            }
        }

        private FsSheetModel? _fsSheet;
        /// <summary>
        /// Danh sách sheet TM trong file excel
        /// </summary>
        public FsSheetModel? SheetModel
        {
            get => _fsSheet;
            set
            {
                SetProperty(ref _fsSheet, value);
            }
        }

        private ObservableCollection<FsNoteModel> _data;
        /// <summary>
        /// Dữ liệu TM trong sheet được chọn từ combobox
        /// </summary>
        public ObservableCollection<FsNoteModel> Data
        {
            get { return _data; }
            set
            {
                SetProperty(ref _data, value);
            }
        }

        private ComboBoxPairs _selectedSheet;
        /// <summary>
        /// Sheet được chọn từ combobox
        /// </summary>
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
        /// <summary>
        /// Danh sách ID chỉ tiêu TM cha chứa TM con
        /// </summary>
        public ObservableGroupedCollection<int, FsNoteModel> GroupedData
        {
            get { return _groupedData; }
            set
            {
                SetProperty(ref _groupedData, value);
            }
        }

        private ObservableCollection<FsNoteModel> _parentNoteData;
        /// <summary>
        /// Danh sách chỉ tiêu TM cha và dữ liệu
        /// </summary>
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
                OnPropertyChanged(nameof(InputText));
                StartDebounceTimer();
            }
        }

        private FsNoteModel? _suggestedFsNoteParent;
        public FsNoteModel? SuggestedFsNoteParent
        {
            get { return _suggestedFsNoteParent; }
            set
            {
                SetProperty(ref _suggestedFsNoteParent, value);
            }
        }

        private ObservableCollection<FsNoteModel> _suggestedFsNoteChildren;
        public ObservableCollection<FsNoteModel> SuggestedFsNoteChildren
        {
            get { return _suggestedFsNoteChildren; }
            set
            {
                SetProperty(ref _suggestedFsNoteChildren, value);
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
        public IRelayCommand OpenAbbyyScreenShotCommand { get; }
        #endregion

        #region Services
        #endregion

        #region Constructors
        public MainWindowViewModel()
        {
            SheetNames = new List<ComboBoxPairs>();
            Data = new ObservableCollection<FsNoteModel>();
            OpenFileCommand = new RelayCommand(OpenFile);
            PasteTextCommand = new RelayCommand(GetDataFromClipboard);
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

        private void GetDataFromClipboard()
        {
            var clipboardText = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardText))
            {
                return;
            }
            InputText = clipboardText;
        }
        /// <summary>
        /// Đọc dữ liệu từ sheet TM được chọn và gán data vào các thuộc tính
        /// </summary>
        /// <param name="sheetName"></param>
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
                    IsLoading = true;
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
            finally
            {
                IsLoading = false;
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

        /// <summary>
        /// Đọc dữ liệu từ sheet Mapping
        /// </summary>
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
            Application.Current.Dispatcher.Invoke(async () =>
            {
                var result = StringUtils.ConvertToMatrix(InputText);
                await HandleData(result);
            });

        }
        /// <summary>
        /// Xử lý dữ liệu input từ clipboard
        /// </summary>
        /// <param name="data"></param>
        private async Task HandleData(List<List<string>> data)
        {
            var moneyList = new List<MoneyCell>();
            var textList = new List<TextCell>();
            double sum = 0L;
            double max = double.MinValue;
            int iRow = 0, iCol = 0;
            data.ForEach(row =>
            {
                row.ForEach(cell =>
                {
                    var text = cell;

                    // Lấy dữ liệu tiền từ ô trong ma trận và xóa dữ liệu tiền khỏi ô
                    GetMoneyAndDeleteFromCell(iRow, iCol, ref sum, ref max, ref moneyList, ref text);

                    text = text.RemoveSign4VietnameseString().RemoveSpecialCharacters();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var textModel = new TextCell
                        {
                            Row = iRow,
                            Col = iCol,
                            Value = text
                        };
                        textList.Add(textModel);
                    }
                    iCol++;
                });
                iRow++;
                iCol = 0;
            });
            var suggestModel = new SuggestModel
            {
                Sum = sum,
                Max = max,
                TextCells = textList,
                MoneyCells = moneyList
            };
            var suggested = await GetParentNoteSuggestions(suggestModel);
            SuggestedFsNoteParent = suggested;

            if (suggested != null)
            {
                var children = Data.Where(x => x.ParentId == suggested.FsNoteId).Select(x => x.DeepClone()).ToList();
                TryIdentifyChildrenNoteWithMoney(ref suggestModel);

                foreach (var moneyCell in suggestModel.MoneyCells!)
                {
                    if (moneyCell.Note == null)
                    {
                        continue;
                    }
                    var noteId = moneyCell.Note.NoteId;
                    var child = children.FirstOrDefault(x => x.FsNoteId == noteId);
                    if (child != null)
                    {
                        child.Value = moneyCell.Value;
                    }
                }

                SuggestedFsNoteChildren = new(children);
            }
            else
            {
                SuggestedFsNoteChildren = [];
            }
        }
        /// <summary>
        /// Lấy dữ liệu tiền từ ô trong ma trận và xóa dữ liệu tiền khỏi ô
        /// </summary>
        /// <param name="iRow"></param>
        /// <param name="iCol"></param>
        /// <param name="sum"></param>
        /// <param name="max"></param>
        /// <param name="moneyList"></param>
        /// <param name="text"></param>
        private static void GetMoneyAndDeleteFromCell(int iRow, int iCol, ref double sum, ref double max, ref List<MoneyCell> moneyList, ref string text)
        {
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
                text = text.Replace(match.Value, string.Empty);
            }
        }

        private async Task<FsNoteModel?> GetParentNoteSuggestions(SuggestModel suggestModel)
        {
            var services = new SuggestServices();

            services.InitSuggest(GroupedData, Data, ParentNoteData, Mapping);

            var parentSuggest1 = services.SuggestParentNoteByTotal(suggestModel); // độ ưu tiên 1
            var parentSuggest3 = await services.SuggestParentNoteByChildren(suggestModel);
            if (parentSuggest1 == null)
            {
                if (parentSuggest3 != null)
                {
                    return parentSuggest3;
                }
                var parentSuggest2 = services.SuggestParentNoteByClosestNumber(suggestModel);
                if (parentSuggest2 != null)
                {
                    return parentSuggest2;
                }
            }
            else
            {
                return (parentSuggest1);
            }
            return null;
        }

        /// <summary>
        /// Xác định các ô chứa dữ liệu tiền và các ô chứa dữ liệu text
        /// </summary>
        private static void TryIdentifyChildrenNoteWithMoney(ref SuggestModel suggestModel)
        {
            Dictionary<string, double> coordinateMoneyDistance = new();
            foreach (var textCell in suggestModel.TextCells!)
            {
                if (textCell.NoteId == 0)
                    continue;

                var minDistance = double.MaxValue;

                // threshold distance arround 2 cells
                const double THRESHOLD = 5;
                MoneyCell? moneyCell2 = null;
                foreach (var moneyCell in suggestModel.MoneyCells!)
                {
                    //string coordinate = $"{moneyCell.Row}-{moneyCell.Col}";
                    //if (coordinateMoneyDistance.ContainsKey(coordinate))
                    //{
                    //    continue;
                    //}
                    var distance = CoreUtils.EuclideanDistance(textCell.Row, textCell.Col, moneyCell.Row, moneyCell.Col);
                    
                    if (distance < minDistance && distance < THRESHOLD)
                    {
                        minDistance = distance;
                        moneyCell2 = moneyCell;
                    }
                }

                if(minDistance != double.MaxValue && moneyCell2 != null)
                {
                    string coordinate = $"{moneyCell2.Row}-{moneyCell2.Col}";
                    if (coordinateMoneyDistance.TryGetValue(coordinate, out double value))
                    {
                        var distance = value;
                        if (minDistance < distance)
                        {
                            coordinateMoneyDistance[coordinate] = minDistance;
                            moneyCell2.Note = new FsNoteCell
                            {
                                NoteId = textCell.NoteId,
                                Row = textCell.Row,
                                Col = textCell.Col,
                            };
                        }
                    }
                    else
                    {
                        coordinateMoneyDistance.Add($"{moneyCell2.Row}-{moneyCell2.Col}", minDistance);
                        moneyCell2.Note = new FsNoteCell
                        {
                            NoteId = textCell.NoteId,
                            Row = textCell.Row,
                            Col = textCell.Col,
                        };
                    }
                }
            }
        }
        #endregion
    }
    public class SuggestModel
    {
        public double Sum { get; set; }
        public double Max { get; set; }
        public List<TextCell>? TextCells { get; set; }
        public List<MoneyCell>? MoneyCells { get; set; }
    }
}

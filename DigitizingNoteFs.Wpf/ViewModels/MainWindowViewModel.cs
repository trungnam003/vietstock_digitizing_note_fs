using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitizingNoteFs.Core.Common;
using DigitizingNoteFs.Core.Models;
using DigitizingNoteFs.Core.ViewModels;
using DigitizingNoteFs.Shared.Utilities;
using DigitizingNoteFs.Wpf.Services;
using Force.DeepCloner;
using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

        private string _filePath = string.Empty;
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

        private FsSheetModel? _fsSheet = null;
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
                if (_selectedSheet != null)
                {
                    LoadDataFromSheet(_selectedSheet.Value);
                }
                else
                {
                    ParentNoteData.Clear();
                }
            }
        }

        private ObservableCollection<FsNoteParentViewModel> _parentNoteData;
        /// <summary>
        /// Danh sách chỉ tiêu TM cha và dữ liệu
        /// </summary>
        public ObservableCollection<FsNoteParentViewModel> ParentNoteData
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

        private FsNoteParentViewModel? _suggestedFsNoteParent;
        public FsNoteParentViewModel? SuggestedFsNoteParent
        {
            get { return _suggestedFsNoteParent; }
            set
            {
                SetProperty(ref _suggestedFsNoteParent, value);
            }
        }

        private ObservableCollection<MoneyCell>? moneyCells;
        public ObservableCollection<MoneyCell>? MoneyCells
        {
            get { return moneyCells; }
            set
            {
                SetProperty(ref moneyCells, value);
            }
        }

        private bool _isAutoMapping;
        public bool IsAutoMapping
        {
            get { return _isAutoMapping; }
            set
            {
                SetProperty(ref _isAutoMapping, value);
            }
        }

        [ObservableProperty]
        private ObservableCollection<MoneyMappingViewModel> moneyMappingData = [];

        partial void OnMoneyMappingDataChanged(ObservableCollection<MoneyMappingViewModel> value)
        {

        }

        public ObservableCollection<ComboBoxPairs> _sheetNames;
        public ObservableCollection<ComboBoxPairs> SheetNames
        {
            get { return _sheetNames; }
            set
            {
                SetProperty(ref _sheetNames, value);
            }
        }

        #endregion

        #region Properties
        public List<ComboBoxPairs> ChildrentNoteSuggests { get; set; } = [];
        public Dictionary<int, List<FsNoteMappingModel>> Mapping { get; set; } = [];
        public HashSet<int> MappingIgnore { get; set; } = [];
        public List<FsNoteModel> ParentNotes { get; set; }

        private HSSFWorkbook? _workbook = null;

        private readonly Timer debounceTimer;
        #endregion

        #region Commands
        public IRelayCommand OpenFileCommand { get; }
        public IRelayCommand PasteTextCommand { get; }
        public IRelayCommand OpenAbbyyScreenShotCommand { get; }
        public IRelayCommand TestCommand { get; }
        #endregion

        #region Services
        #endregion

        #region Constructors
        public MainWindowViewModel()
        {
            SheetNames = [];
            ParentNoteData = [];
            OpenFileCommand = new RelayCommand(OpenFile);
            PasteTextCommand = new RelayCommand(GetDataFromClipboard);
            OpenAbbyyScreenShotCommand = new RelayCommand<object>(OpenAbbyyScreenShot);
            TestCommand = new RelayCommand(() =>
            {
                var d = MoneyMappingData;
                foreach (var item in MoneyMappingData)
                {
                    item.SelectedNoteFsChild = item.NoteFsChildren.Last();
                }
            });
            debounceTimer = new Timer(DebounceTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }
        #endregion

    }

    internal partial class MainWindowViewModel
    {
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

        private static bool IsValidCell(IRow row, int colName, int colNoteId)
        {

            if (row == null)
            {
                return false;
            }

            string? name = row.GetCell(colName).ToString();
            // kiểm tra nếu ô màu đỏ thì bỏ qua
            var cellColor = row.GetCell(colName).CellStyle.FillForegroundColorColor;
            Color colorTarget = Color.FromArgb(cellColor.RGB[0], cellColor.RGB[1], cellColor.RGB[2]);

            bool validName = !string.IsNullOrEmpty(name);
            bool validNoteId = int.TryParse(row.GetCell(colNoteId).ToString(), out int noteId) && noteId != 0;
            bool validColor = !CoreUtils.IsColorInRangeRed(colorTarget);

            return validName && validNoteId && validColor;
        }

        /// <summary>
        /// Đọc dữ liệu từ sheet TM được chọn và gán data vào các thuộc tính
        /// </summary>
        /// <param name="sheetName"></param>
        private void LoadDataFromSheet(string sheetName)
        {
            if (string.IsNullOrEmpty(sheetName))
                return;

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
            Dictionary<int, int> countGroup = [];
            FsNoteParentViewModel? currentParent = null;
            for (int i = START_ROW_INDEX; i <= sheet.LastRowNum; i++)
            {
                try
                {
                    var row = sheet.GetRow(i);
                    if (row == null)
                    {
                        continue;
                    }
                    if (!IsValidCell(row, COL_NOTE_NAME, COL_NOTE_ID))
                    {
                        continue;
                    }
                    int noteId = (int)row.GetCell(COL_NOTE_ID).NumericCellValue;
                    string name = row.GetCell(COL_NOTE_NAME).ToString()!;

                    var cellCheckParent = row.GetCell(COL_CHECK_PARENT_NOTE);

                    if (cellCheckParent == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(cellCheckParent.ToString()))
                    {
                        if (!Mapping.ContainsKey(noteId))
                        {
                            continue;
                        }
                        currentParent = new FsNoteParentViewModel
                        {
                            FsNoteId = noteId,
                            Name = name,
                            IsParent = true,
                            ParentId = 0,
                            Group = 0,
                            Cell = new(i, COL_NOTE_VALUE),
                            CellAddress = $"{(char)('A' + COL_NOTE_VALUE)}{i}",
                            Children = []
                        };
                        currentParentId = noteId;
                        if (countGroup.TryGetValue(currentParentId, out int value))
                        {
                            countGroup[currentParentId] = ++value;
                        }
                        else
                        {
                            countGroup[currentParentId] = 1;
                        }
                        var cellParentValue = row.GetCell(COL_NOTE_PARENT_VALUE);
                        if (cellParentValue == null)
                        {
                            currentParent.Value = 0;
                        }
                        else
                        {
                            currentParent.Value = cellParentValue.NumericCellValue;
                        }
                        currentParent.Group = countGroup.TryGetValue(currentParentId, out int group) ? group : 0;
                        ParentNoteData.Add(currentParent);
                    }
                    else
                    {
                        var cellValue = row.GetCell(COL_NOTE_VALUE);
                        if (cellValue.CellType == CellType.Formula)
                        {
                            continue;
                        }
                        var model = new FsNoteViewModel
                        {
                            FsNoteId = noteId,
                            Name = name,
                            IsParent = false,
                            ParentId = currentParentId,
                            Cell = new(i, COL_NOTE_VALUE),
                            CellAddress = $"{(char)('A' + COL_NOTE_VALUE)}{i}",
                            Group = countGroup.TryGetValue(currentParentId, out int group) ? group : 0
                        };
                        currentParent?.Children.Add(model);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

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
                    LoadMappingData();
                    LoadSheetNames();
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
                if (SheetNames.Count > 0)
                {
                    SelectedSheet = SheetNames[0];
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

                if (sheet == null)
                {
                    MessageBox.Show("Mapping?");
                    return;
                }

                const string IGNORE_CHILD_FORMULA_KEY = "#";
                const string IGNORE_PARENT_KEY = "x";
                const string OTHER_KEY = "*";

                const int START_ROW = 1;
                const int COL_NOTE_ID = 0;
                const int COL_KEYWORD = 2;
                const int COL_IS_PARENT = 3;

                int currentParentId = 0;
                Dictionary<int, int> countGroup = [];
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
                        var prevParentId = currentParentId;
                        currentParentId = (int)cellNoteId.NumericCellValue;
                        if (prevParentId != 0 && prevParentId == currentParentId)
                        {
                            countGroup[prevParentId] = countGroup.TryGetValue(prevParentId, out int value) ? value + 1 : 1;
                        }
                        if (!Mapping.ContainsKey(currentParentId))
                        {
                            Mapping[currentParentId] = [];
                            countGroup[currentParentId] = 1;
                        }
                        if (cellKeyword != null && cellKeyword.StringCellValue.ToLower().Equals(IGNORE_PARENT_KEY))
                        {
                            _ = MappingIgnore.Add(currentParentId);
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
                        if (rawKeyword.Equals(OTHER_KEY))
                        {
                            model.IsOther = true;
                        }
                        else if (rawKeyword.Equals(IGNORE_CHILD_FORMULA_KEY))
                        {
                            model.IsFormula = true;
                        }
                        else
                        {
                            var keywords = new List<string>(rawKeyword.Split(','));
                            model.Keywords = keywords.Select(x => x.Trim()).ToList();
                        }
                    }

                    model.ParentId = currentParentId;
                    model.Group = countGroup.TryGetValue(currentParentId, out int group) ? group : 0;
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
                if (string.IsNullOrWhiteSpace(InputText))
                {
                    return;
                }
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

                    text = text.RemoveSign4VietnameseString().RemoveSpecialCharacters().Trim();

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
            // cân nhắc khi sử dụng

            if (suggested != null)
            {
                var parent =  ParentNoteData.Where(x => x.FsNoteId == suggested?.FsNoteId).FirstOrDefault();

                if(parent == null)
                {
                    return;
                }

                SuggestedFsNoteParent = parent.ShallowClone();
                parent.Children = parent.Children.Select(x => x.ShallowClone()).ToList();

                var count = TryIdentifyChidrenNoteWithText(ref suggestModel, suggested);
                var children = SuggestedFsNoteParent!.Children;

                ChildrentNoteSuggests = children.Select(x => new ComboBoxPairs(x.FsNoteId.ToString(), x.Name!)).ToList();

                if (count > 0)
                {
                    TryIdentifyChildrenNoteWithMoney(ref suggestModel);

                    foreach (var moneyCell in suggestModel.MoneyCells!)
                    {
                        if (moneyCell.Note == null)
                        {
                            continue;
                        }
                        var noteId = moneyCell.Note.NoteId;
                        var child = children?.FirstOrDefault(x => x.FsNoteId == noteId);
                        if (child != null)
                        {
                            child.Value = moneyCell.Value;
                        }
                    }
                }
                // loop and set selected parent note
                foreach (var item in ParentNoteData)
                {
                    if (item.FsNoteId == suggested.FsNoteId)
                    {
                        item.IsSelected = true;
                    }
                }
            }
            else
            {
                // loop and set selected parent note
                foreach (var parent in ParentNoteData)
                {
                    parent.IsSelected = false;
                }

            }
            // map money
            MoneyCells = new(moneyList);
            MoneyMappingData.Clear();
            foreach (var money in moneyList)
            {
                var vm = new MoneyMappingViewModel
                {
                    Value = money.Value,
                    NoteId = money.Note?.NoteId ?? 0,
                    NoteFsChildren = new(ChildrentNoteSuggests.Select(x => new ComboBoxPairs(x.Key, x.Value)).ToList()),
                };
                vm.SelectedNoteFsChildChanged +=
                    (object? sender, EventArgs e) =>
                    {
                        if(sender is MoneyMappingViewModel vm2)
                        {
                            int noteId = int.Parse(vm2.SelectedNoteFsChild?.Key ?? "0");
                            var child = SuggestedFsNoteParent?.Children.Where(x => x.FsNoteId == noteId).FirstOrDefault();
                            if (child != null)
                            {
                                child.Value += vm2.Value;
                            }
                        }
                    };

                vm.SelectedNoteFsChildChanging += Vm_SelectedNoteFsChildChanging;
                MoneyMappingData.Add(vm);
            }

            var d = MoneyMappingData;

            await Task.Run(async () =>
            {
                // wait for 100ms to UI update
                await Task.Delay(100);

                foreach (var item in MoneyMappingData)
                {
                    item.SelectedNoteFsChild = item.NoteFsChildren.Where(y => y.Key == item.NoteId.ToString()).FirstOrDefault();
                }
            });

        }

        private void Vm_SelectedNoteFsChildChanging(object? sender, EventArgs e)
        {
            if (sender is MoneyMappingViewModel vm)
            {
                var noteId = int.Parse(vm.SelectedNoteFsChild?.Key ?? "0");
                var child = SuggestedFsNoteParent?.Children.Where(x => x.FsNoteId == noteId).FirstOrDefault();
                if (child != null)
                {
                    child.Value -= vm.Value;
                }
            }
        }

        /// <summary>
        /// Get data money using Regex from cell and remove money from cell
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

        private async Task<FsNoteParentViewModel?> GetParentNoteSuggestions(SuggestModel suggestModel)
        {
            var services = new SuggestServices();

            services.InitSuggest(ParentNoteData, Mapping, MappingIgnore);

            var parentSuggest1 = services.SuggestParentNoteByTotal(suggestModel);
            if (parentSuggest1 == null)
            {
                var parentSuggest3 = await services.SuggestParentNoteByChildren(suggestModel);
                if (parentSuggest3 != null)
                {
                    return parentSuggest3;
                }
            }
            else
            {
                return (parentSuggest1);
            }
            return null;
        }

        /// <summary>
        /// Xác định các ô chứa dữ liệu text phù hợp với các TM nào
        /// </summary>
        /// <param name="suggestModel"></param>
        private int TryIdentifyChidrenNoteWithText(ref SuggestModel suggestModel, FsNoteViewModel suggested)
        {
            if (suggestModel.TextCells == null || suggestModel.TextCells.Count == 0)
                return 0;
            var childrenNotes = Mapping[suggested.FsNoteId].Where(x => x.Keywords.Count > 0 && !x.IsFormula && !x.IsOther).ToList();
            const double THRESHOLD = 0.65;

            foreach (var childNote in childrenNotes)
            {
                foreach (var textCell in suggestModel.TextCells)
                {
                    var text = textCell.Value;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }
                    double maxSimilarity = 0;

                    foreach (var keyword in childNote.Keywords)
                    {
                        double currentSimilarity = StringSimilarityUtils.CalculateSimilarity(keyword, text);
                        if (currentSimilarity > maxSimilarity)
                        {
                            maxSimilarity = currentSimilarity;
                        }
                        if (maxSimilarity >= THRESHOLD)
                        {
                            break;
                        }
                    }
                    // Nếu maxSimilarity > THRESHOLD thì xác định đây là note con của parentNote
                    if (maxSimilarity >= THRESHOLD && maxSimilarity > textCell.Similarity)
                    {
                        textCell.NoteId = childNote.Id;
                        textCell.Similarity = maxSimilarity;
                    }
                }
            }
            return suggestModel.TextCells.Where(x => x.NoteId > 0).ToList().Count;
        }

        /// <summary>
        /// Xác định các ô chứa dữ liệu tiền phù hợp với các ô chứa dữ liệu text
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

                if (minDistance != double.MaxValue && moneyCell2 != null)
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
}

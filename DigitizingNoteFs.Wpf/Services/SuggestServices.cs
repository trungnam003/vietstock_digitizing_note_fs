using CommunityToolkit.Mvvm.Collections;
using DigitizingNoteFs.Core.Models;
using DigitizingNoteFs.Shared.Utilities;
using DigitizingNoteFs.Wpf.ViewModels;
using Force.DeepCloner;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DigitizingNoteFs.Wpf.Services
{
    public class SuggestServices
    {
        public ObservableGroupedCollection<int, FsNoteModel>? GroupedData { get; private set; }
        public ObservableCollection<FsNoteModel>? Data { get; private set; }
        public ObservableCollection<FsNoteModel>? ParentNoteData { get; private set; }
        public Dictionary<int, List<FsNoteMappingModel>>? Mapping { get; private set; }

        public bool Inintialized => GroupedData != null && Data != null && ParentNoteData != null && Mapping != null;

        public void InitSuggest(

            ObservableGroupedCollection<int, FsNoteModel> groupedData,
            ObservableCollection<FsNoteModel> data,
            ObservableCollection<FsNoteModel> parentNoteData,
            Dictionary<int, List<FsNoteMappingModel>> mapping

            )
        {

            GroupedData = groupedData;
            Data = data;
            ParentNoteData = parentNoteData;
            Mapping = mapping;

        }
        public FsNoteModel? SuggestParentNoteByTotal(SuggestModel suggestModel)
        {
            if(suggestModel.MoneyCells == null || suggestModel.MoneyCells.Count == 0 || suggestModel.Max == double.MinValue)
            {
                return null;
            }
            var sum = suggestModel.Sum;
            var max = suggestModel.Max;
            var isUserScanTotal = sum - max == max;

            if (isUserScanTotal)
            {
                var parentNote = ParentNoteData!.FirstOrDefault(x => x.Value == max);
                if (parentNote != null)
                {
                    return (parentNote);
                }
            }
            else
            {
                var parentNote = ParentNoteData!.FirstOrDefault(x => x.Value == sum);
                if (parentNote != null)
                {
                    return (parentNote);
                }
            }

            return null;
        }

        public Task<FsNoteModel?> SuggestParentNoteByChildren(SuggestModel suggestModel)
        {
            FsNoteModel? parentNoteRs = null;
            const double THRESHOLD = 0.7;

            if(!Inintialized)
            {
                return Task.FromResult(parentNoteRs);
            }

            if (suggestModel.TextCells == null || suggestModel.TextCells.Count == 0)
                return Task.FromResult(parentNoteRs);

            const double ZERO_RATE = 0.0;
            var maxRate = ZERO_RATE;
            int parentIdWithMaxRate = 0;

            foreach (var parentNote in Mapping!)
            {
                var total = parentNote.Value.Count;
                var countSimilarity = 0;
                var childrenNotes = parentNote.Value;
                //List<TextCell> cloneTextCells = suggestModel.TextCells.Select(x => x.DeepClone()).ToList();
                // store textCell coordinates and current similarity
                var textCellCoordinates = new Dictionary<string, double>();
                foreach (var childNote in childrenNotes)
                {
                    //List<TextCell> cloneTextCells = suggestModel.TextCells.Select(x => x.DeepClone()).ToList();
                    foreach (var textCell in suggestModel.TextCells)
                    {
                        var coordinate = $"{textCell.Row}-{textCell.Col}";

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
                            countSimilarity++;
                        }
                    }
                }

                var rate = (double)countSimilarity / total;
                if (rate > maxRate)
                {
                    maxRate = rate;
                    parentIdWithMaxRate = parentNote.Key;
                }
                else
                {
                    // clear textCells
                    foreach (var textCell in suggestModel.TextCells)
                    {
                        textCell.NoteId = 0;
                        textCell.Similarity = 0.0;
                    }
                }

            }

            if (maxRate != ZERO_RATE)
            {
                parentNoteRs = ParentNoteData!.FirstOrDefault(x => x.FsNoteId == parentIdWithMaxRate);
                if (parentNoteRs != null)
                {
                    return Task.FromResult<FsNoteModel?>(parentNoteRs);
                }
            }

            return Task.FromResult<FsNoteModel?>(parentNoteRs);
        }

        public FsNoteModel? SuggestParentNoteByClosestNumber(SuggestModel suggestModel)
        {
            const double _30M = 30_000_000;
            if (suggestModel.MoneyCells == null || suggestModel.MoneyCells.Count == 0 || suggestModel.Max == double.MinValue)
            {
                return null;
            }
            var sum = suggestModel.Sum;
            var max = suggestModel.Max;
            var isUserScanTotal = sum - max == max;
            List<double> values = ParentNoteData!.Select(x => x.Value).ToList();
            if (isUserScanTotal)
            {
                // Tìm số gần đúng nhất với max trong list và chênh lệch tối thiểu là 30_000_000 (30 triệu)
                var (closest, isFound) = CoreUtils.FindClosestNumber(max, values, _30M);
                if (isFound)
                {
                    var parentNote = ParentNoteData!.FirstOrDefault(x => x.Value == closest);
                    if (parentNote != null)
                    {
                        return (parentNote);
                    }
                }
            }
            else
            {
                // Tìm số gần đúng nhất với sum trong list và chênh lệch tối thiểu là 30_000_000 (30 triệu)
                var (closest, isFound) = CoreUtils.FindClosestNumber(sum, values, _30M);
                if (isFound)
                {
                    var parentNote = ParentNoteData!.FirstOrDefault(x => x.Value == closest);
                    if (parentNote != null)
                    {
                        return (parentNote);
                    }
                }
            }

            return null;
        }
    }
}

using CommunityToolkit.Mvvm.Collections;
using DigitizingNoteFs.Core.Models;
using DigitizingNoteFs.Shared.Utilities;
using DigitizingNoteFs.Wpf.ViewModels;
using System.Collections.ObjectModel;

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

        public FsNoteModel? SuggestParentNoteByChildren(SuggestModel suggestModel)
        {
            const double THRESHOLD = 0.7;
            if(!Inintialized)
            {
                return null;
            }
            if (suggestModel.TextCells == null || suggestModel.TextCells.Count == 0)
                return null;
            var maxRate = 0.0;
            int parentIdWithMaxRate = 0;
            foreach (var parentNote in Mapping!)
            {
                var total = parentNote.Value.Count;
                var countSimilarity = 0;
                parentNote.Value.ForEach(childrenNote =>
                {
                    foreach (var textCell in suggestModel.TextCells)
                    {
                        var text = textCell.Value;
                        if(string.IsNullOrWhiteSpace(text))
                        {
                            continue;
                        }
                        double maxSimilarity = 0;
                        childrenNote.Keywords.ForEach(keyword =>
                        {
                            double currentSimilarity = StringSimilarityUtils.CalculateSimilarity(keyword, text);
                            if (currentSimilarity > maxSimilarity)
                            {
                                maxSimilarity = currentSimilarity;
                            }
                        });

                        // Nếu maxSimilarity > THRESHOLD thì xác định đây là note con của parentNote
                        if (maxSimilarity > THRESHOLD)
                        {
                            countSimilarity++;
                        }
                    }
                });
                var rate = (double)countSimilarity / total;
                if (rate > maxRate)
                {
                    maxRate = rate;
                    parentIdWithMaxRate = parentNote.Key;
                }
            }
            
            if (parentIdWithMaxRate != 0.0)
            {
                var parentNote = ParentNoteData!.FirstOrDefault(x => x.FsNoteId == parentIdWithMaxRate);
                if (parentNote != null)
                {
                    return (parentNote);
                }
            }

            return null;
        }

        public FsNoteModel? SuggestParentNoteByClosestNumber(SuggestModel suggestModel)
        {
            var sum = suggestModel.Sum;
            var max = suggestModel.Max;
            var isUserScanTotal = sum - max == max;
            List<double> values = ParentNoteData!.Select(x => x.Value).ToList();
            if (isUserScanTotal)
            {
                // Tìm số gần đúng nhất với max trong list và chênh lệch tối thiểu là 30_000_000 (30 triệu)
                var (closest, isFound) = CoreUtils.FindClosestNumber(max, values, 30_000_000L);
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
                var (closest, isFound) = CoreUtils.FindClosestNumber(sum, values, 30_000_000L);
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

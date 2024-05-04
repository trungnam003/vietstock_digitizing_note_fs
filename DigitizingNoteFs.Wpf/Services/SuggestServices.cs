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
        
        public ObservableCollection<FsNoteModel>? ParentNoteData { get; private set; }
        public Dictionary<int, List<FsNoteMappingModel>>? Mapping { get; private set; }
        public HashSet<int>? MappingIgnore { get; private set; }

        public bool Inintialized => ParentNoteData != null && Mapping != null && MappingIgnore != null;

        public void InitSuggest(

            ObservableCollection<FsNoteModel> parentNoteData,
            Dictionary<int, List<FsNoteMappingModel>> mapping,
            HashSet<int> mappingIgnore

            )
        {

            ParentNoteData = parentNoteData;
            Mapping = mapping;
            MappingIgnore = mappingIgnore;

        }
        public FsNoteModel? SuggestParentNoteByTotal(SuggestModel suggestModel)
        {
            if (!Inintialized)
            {
                return null;
            }
            if (suggestModel.MoneyCells == null || suggestModel.MoneyCells.Count == 0 || suggestModel.Max == double.MinValue)
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
        /// <summary>
        /// Xác định parentNote dựa trên các note con
        /// </summary>
        /// <param name="suggestModel"></param>
        /// <returns></returns>
        public Task<FsNoteModel?> SuggestParentNoteByChildren(SuggestModel suggestModel)
        {
            FsNoteModel? parentNoteRs = null;
            const double THRESHOLD = 0.65;

            if(!Inintialized)
            {
                return Task.FromResult(parentNoteRs);
            }

            if (suggestModel.TextCells == null || suggestModel.TextCells.Count == 0)
                return Task.FromResult(parentNoteRs);

            const double ZERO_RATE = 0.0;
            var maxRate = ZERO_RATE;
            int parentIdWithMaxRate = 0;

            var cloneTextCells = suggestModel.TextCells.Select(x => x.DeepClone()).ToList();
            foreach (var parentNote in Mapping!)
            {
                var total = cloneTextCells.Count;
                var countSimilarity = 0;
                var childrenNotes = parentNote.Value.Where(x => x.Keywords.Count > 0 && !x.IsFormula && !x.IsOther).ToList();

                if (MappingIgnore!.Contains(parentNote.Key))
                {
                    continue;
                }

                foreach (var childNote in childrenNotes)
                {
                    foreach (var textCell in cloneTextCells)
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
                            if (textCell.NoteId == 0)
                            {
                                countSimilarity++;
                            }
                            textCell.NoteId = childNote.Id;
                            textCell.Similarity = maxSimilarity;
                        }
                    }
                }

                var rate = (double)countSimilarity / total;
                if (rate > maxRate)
                {
                    maxRate = rate;
                    parentIdWithMaxRate = parentNote.Key;
                }

                // clear textCells
                cloneTextCells.ForEach(x =>
                {
                    x.NoteId = 0;
                    x.Similarity = 0;
                });
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
        [Obsolete]
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

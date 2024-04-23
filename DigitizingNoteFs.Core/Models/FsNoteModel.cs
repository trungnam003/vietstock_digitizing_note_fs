
namespace DigitizingNoteFs.Core.Models
{
    public class FsNoteModel
    {
        public string? CellAddress { get; set; }
        public Tuple<int, int>? Cell { get; set; }
        public string? Name { get; set; }
        public double Value { get; set; }
        public int FsNoteId { get; set; }
        public int ParentId { get; set; }
        public bool IsParent { get; set; }
    }
}

namespace DigitizingNoteFs.Core.Models
{
    public class MatrixCell
    {
        public int Row { get; set; }
        public int Col { get; set; }
    }
    public class MoneyCell : MatrixCell
    {
        public double Value { get; set; }
        public FsNoteCell? Note { get; set; }
    }

    public class TextCell : MatrixCell
    {
        public string? Value { get; set; }
    }

    public class FsNoteCell : MatrixCell
    {
        public int NoteId { get; set; }
        public int ParentId { get; set; }
    }
}

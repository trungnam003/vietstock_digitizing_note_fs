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
        public int NoteId { get; set; }
        public double Similarity { get; set; } = 0.0;
    }

    public class FsNoteCell : MatrixCell
    {
        public int NoteId { get; set; }
        public int ParentId { get; set; }
    }
}

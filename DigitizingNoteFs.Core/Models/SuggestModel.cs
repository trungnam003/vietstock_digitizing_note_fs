namespace DigitizingNoteFs.Core.Models
{
    public class SuggestModel
    {
        public double Sum { get; set; }
        public double Max { get; set; }
        public List<TextCell>? TextCells { get; set; }
        public List<MoneyCell>? MoneyCells { get; set; }
    }
}

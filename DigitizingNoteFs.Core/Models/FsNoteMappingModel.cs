

namespace DigitizingNoteFs.Core.Models
{
    /// <summary>
    /// Lưu trữ metadata của một note
    /// (keywords, group, ignore, other)
    /// </summary>
    public class FsNoteMappingModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = [];
        public int Group { get; set; }
        public bool IsFormula { get; set; }
        public bool IsOther { get; set; }
    }
}

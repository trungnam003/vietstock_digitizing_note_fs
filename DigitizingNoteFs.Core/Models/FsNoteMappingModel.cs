

namespace DigitizingNoteFs.Core.Models
{
    public class FsNoteMappingModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = [];
        public int Group { get; set; }
    }
}

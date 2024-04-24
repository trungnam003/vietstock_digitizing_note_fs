using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitizingNoteFs.Core.Models
{
    public class MatrixCell
    {
        public int Row { get; set; }
        public int Col { get; set; }
    }
    public class MoneyCell : MatrixCell
    {
        public long Value { get; set; }
        public FsNoteCell? Note { get; set; }
    }

    public class FsNoteCell : MatrixCell
    {
        public int NoteId { get; set; }
        public int ParentId { get; set; }
    }
}

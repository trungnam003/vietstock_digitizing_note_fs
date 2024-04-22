using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitizingNoteFs.Core.Models
{
    public class FsSheetModel
    {
        public string? StockCode { get; set; }
        public string? ReportTerm { get; set; }
        public string? Year { get; set; }
        public string? AuditedStatus { get; set; }
        public string? IsAdjusted { get; set; }
        public string? ReportType { get; set; }
    }
}

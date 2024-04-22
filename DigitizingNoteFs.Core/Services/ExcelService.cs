
using DigitizingNoteFs.Core.Models;
using NPOI.SS.UserModel;
using System.IO;


namespace DigitizingNoteFs.Core.Services
{
    public class ExcelService
    {
        public const int FCol = 5;
        public FsSheetModel? ReadImportFsExcelFile(string filePath, string sheetName)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var workbook = WorkbookFactory.Create(fs);

            // check if sheetName exists
            var sheet = workbook.GetSheet(sheetName);
            if (sheet == null)
            {
                return null;
            }

            var fsSheetModel = new FsSheetModel();

            fsSheetModel.StockCode = sheet.GetRow(2).GetCell(FCol).StringCellValue;
            fsSheetModel.ReportTerm = sheet.GetRow(3).GetCell(FCol).StringCellValue;
            fsSheetModel.Year = sheet.GetRow(4).GetCell(FCol).NumericCellValue.ToString();
            fsSheetModel.AuditedStatus = sheet.GetRow(5).GetCell(FCol).StringCellValue;
            fsSheetModel.ReportType = sheet.GetRow(6).GetCell(FCol).StringCellValue;
            fsSheetModel.IsAdjusted = sheet.GetRow(8).GetCell(FCol).StringCellValue;

            return fsSheetModel;
            
        }
    }
}

using ConflictAutomation.Constants;
using ConflictAutomation.Extensions;
using ConflictAutomation.Models.FinScan;
using ConflictAutomation.Utilities;
using OfficeOpenXml;

namespace ConflictAutomation.Services.FinScan;

public class FinScanMatchReportExcelFormatter
{
    private readonly string _templateWorkbookPath;


    public FinScanMatchReportExcelFormatter(string templateWorkbookFullPath)
    {
        if (!File.Exists(templateWorkbookFullPath))
        {
            throw new FileNotFoundException($"FinScanMatchReportExcelFormatter: Template workbook file not found at {templateWorkbookFullPath}");
        }
        _templateWorkbookPath = templateWorkbookFullPath;
    }


    public string Run(string targetWorkbookFullPath, FinScanMatchReport finScanMatchReport)
    {
        FileSystem.DeleteFileIfExisting(targetWorkbookFullPath);
        ArgumentNullException.ThrowIfNull(finScanMatchReport);
        CreateTargetFileFromTemplate(targetWorkbookFullPath);

        using (var excelPackage = new ExcelPackage(targetWorkbookFullPath))
        {
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];

            worksheet.Cells["A3"].Value = finScanMatchReport.UserOrganizationCreatedTimestamp;

            worksheet.Cells["D20"].Value = finScanMatchReport.ApplicationName;
            worksheet.Cells["D22"].Value = finScanMatchReport.RecordType;
            worksheet.Cells["D23"].Value = finScanMatchReport.Name;
            worksheet.Cells["N20"].WriteData(FormatClientId(finScanMatchReport.ClientId), CAUConstants.SMALL_FONT_SIZE);
            worksheet.Cells["N22"].Value = finScanMatchReport.Address;
            worksheet.Cells["N24"].Value = finScanMatchReport.Notes;

            int row = 27;
            foreach (var category in finScanMatchReport.ListCategories)
            {
                worksheet.Cells[$"A{row}"].Value = category.ListName;
                worksheet.Cells[$"I{row}"].Value = category.CategoryName;
                row++;
            }

            worksheet.Cells["A39"].Value = finScanMatchReport.NumberOfRecordsReported;
            worksheet.Cells["G39"].Value = finScanMatchReport.NumberOfRecordsReturned;
            worksheet.Cells["L39"].Value = finScanMatchReport.NumberOfRecordsNotReturned;

            row = 45;
            foreach (var result in finScanMatchReport.ListFullResultSet)
            {
                worksheet.Cells[$"A{row}:B{row}"].MergeAndWrap(result.ListName);
                worksheet.Cells[$"C{row}:E{row}"].MergeAndWrap(result.ListProfileId);
                worksheet.Cells[$"F{row}:J{row}"].MergeAndWrap(result.ClientNameAndAddress);
                worksheet.Cells[$"K{row}:L{row}"].MergeAndWrap(result.Country);
                worksheet.Cells[$"M{row}:Q{row}"].MergeAndWrap(result.Version);
                worksheet.Cells[$"R{row}"].WriteData(result.MatchString);
                worksheet.Cells[$"S{row}"].WriteData(result.RecordType);

                worksheet.AutoFitRowHeight(row, startCol: 'A', endCol: 'S', 
                    allMergedCols: [('A', 'B'), ('C', 'E'), ('F', 'J'), ('K', 'L'), ('M', 'Q')],
                    forceWrapText: true, minHeight: CAUConstants.STD_MIN_HEIGHT);

                row++;
            }

            excelPackage.Save();
        }

        return Path.Exists(targetWorkbookFullPath) ? targetWorkbookFullPath : string.Empty;
    }


    private static string FormatClientId(string clientId)
    {
        string sep = FinScanConstants.STRING_SEPARATOR.FullTrim();
        const string UNTIL = "»";

        var edges = ((List<string>)
                     [$"{clientId}{sep}".StrLeft(sep).FullTrim(), 
                      $"{sep}{clientId}".StrRightBack(sep).FullTrim()]).Distinct().ToList();        

        return string.Join(UNTIL, edges);
    }


    private void CreateTargetFileFromTemplate(string targetFilePath)
    {
        try
        {
            File.Copy(_templateWorkbookPath, targetFilePath, overwrite: true);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create target file {targetFilePath}  from template file {_templateWorkbookPath}", ex);
        }
    }

}

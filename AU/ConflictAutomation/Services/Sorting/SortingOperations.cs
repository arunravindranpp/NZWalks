using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using OfficeOpenXml;
using Serilog;
using MWP = ConflictAutomation.Services.MasterWorkbookParser;

namespace ConflictAutomation.Services.Sorting;

#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0305 // Simplify collection initialization
public static class SortingOperations
{
    public static List<string> CAUSort(this List<string> inputList)
    {
        if (inputList.IsNullOrEmpty())
        {
            return [];
        }

        return inputList.OrderBy(item => CauSequenceKey(item)).ToList();
    }


    public static List<ResearchSummary> CAUSort(this List<ResearchSummary> inputList)
    {
        if (inputList.IsNullOrEmpty())
        {
            return [];
        }

        return inputList.OrderBy(item => CauSequenceKey(item.WorksheetNo)).ToList();
    }


    public static string CauSequenceKey(string tabName)
    {
        if (string.IsNullOrEmpty(tabName))
        {
            return string.Empty;
        }

        string result = tabName
                          .Replace(".P", ".01.", StringComparison.OrdinalIgnoreCase)
                          .Replace(".G", ".02.", StringComparison.OrdinalIgnoreCase)
                          .Replace(".C", ".03.", StringComparison.OrdinalIgnoreCase)
                          .Replace(".A", ".04.", StringComparison.OrdinalIgnoreCase)
                          .Replace(".D", ".05.", StringComparison.OrdinalIgnoreCase);

        return result;
    }


    public static void SortWorksheets(string filePath)
    {
        try
        {
            using (var package = new ExcelPackage(filePath))
            {
                MWP.MasterWorkbookParser masterWorkbookParser = new(package.Workbook);
                List<string> unitGridTabNames = masterWorkbookParser.UnitGridWorksheetTabNames;

                List<string> sortedUnitGridTabNames = SortingOperations.CAUSort(unitGridTabNames);

                package.Workbook.Worksheets.Sort(sortedUnitGridTabNames);
                package.Save();
            }
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred when executing method SortingOperations.SortWorksheets() - Message: {ex}");
            LoggerInfo.LogException(ex);
        }
    }
}
#pragma warning restore IDE0305 // Simplify collection initialization
#pragma warning restore IDE0063 // Use simple 'using' statement

using OfficeOpenXml;

namespace ConflictAutomation.Extensions;

public static class ExcelWorksheetsExtensions
{
    // Remember to save the spreadsheet after calling this method
    public static void Sort(this ExcelWorksheets worksheetsColl, List<string> sortedTabNames)
    {
        ArgumentNullException.ThrowIfNull(worksheetsColl);

        if (sortedTabNames.IsNullOrEmpty())
        {
            return;
        }
        if (sortedTabNames.Count < 2)
        {
            return;
        }

        for (int i = 1; i < sortedTabNames.Count; i++)
        {
            var tabName = sortedTabNames[i];
            var previousTabName = sortedTabNames[i - 1];
            worksheetsColl.MoveAfter(tabName, previousTabName);
        }
    }
}

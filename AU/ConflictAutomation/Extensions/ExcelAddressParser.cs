namespace ConflictAutomation.Extensions;

public static class ExcelAddressParser
{
    public static int GetRow(this string reference) => reference.GetStartRow();

    public static int GetCol(this string reference) => reference.GetStartCol();

    public static int GetStartCol(this string reference) =>
        Int32.Parse($"{reference}:".StrLeft(":").GetLettersOnly());

    public static int GetStartRow(this string reference) => 
        Int32.Parse($"{reference}:".StrLeft(":").GetDigitsOnly());

    public static int GetEndCol(this string reference) =>
        Int32.Parse($":{reference}".StrRight(":").GetLettersOnly());

    public static int GetEndRow(this string reference) =>
        Int32.Parse($":{reference}".StrRight(":").GetDigitsOnly());
}

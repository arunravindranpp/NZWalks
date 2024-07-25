namespace ConflictAutomation.Extensions;

public static class IntExtensions
{
    public static char ExcelColumnLetter(this int columnNumber) => (char)('A' + (char)(columnNumber - 1));
}

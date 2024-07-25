namespace ConflictAutomation.Extensions;

public static class CharExtensions
{
    public static int ExcelColumnNumber(this char columnLetter) => columnLetter - 'A' + 1;


    public static bool IsDigit(this char c) => ('0' <= c) && (c <= '9');


    public static bool IsLetter(this char c) => (('a' <= c) && (c <= 'z')) || (('A' <= c) && (c <= 'Z'));
}

namespace ConflictAutomation.Extensions;

public static class IEnumerableExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list) =>
        list is null || !list.Any();


    public static bool HaveAMinimumLengthOf(this IEnumerable<string> listStrings, int minimalLength)
    {
        if (listStrings.IsNullOrEmpty())
        {
            return false;
        }

        return !listStrings!.Any(s => s.Length < minimalLength);
    }
}

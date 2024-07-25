namespace KeyGen.tests.Extensions;

internal static class ListStringExtensions
{
    public static string AsString(this List<string>? listStrings, string sep = "; ")
    {
        if ((listStrings is null) || (listStrings.Count == 0))
        {
            return string.Empty;
        }

        return string.Join(sep, listStrings!);
    }   
}

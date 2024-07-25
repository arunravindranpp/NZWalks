namespace ConflictAutomation.Extensions;

public static class DictionaryExtensions
{
    public static Dictionary<string, string> SortByLengthDescending(this Dictionary<string, string> input) =>
        input
            .OrderByDescending(pair => pair.Key.Length)
            // .ThenBy(pair => pair.Key)
            .ToDictionary(pair => pair.Key, pair => pair.Value);


    public static bool ContainsAnyOfSubstringReplacements(this string name, Dictionary<string, string> replacements) =>
        $" {name} ".Contains(replacements.Select(r => $" {r.Key.Trim()} "));
}

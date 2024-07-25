using System.Collections.Generic;

namespace ConflictAutomation.Extensions;

public static class ListStringExtensions
{
    public static object AppendNewElementsOnly(this List<string> originalList, List<string> additionalElements)
    {
        ArgumentNullException.ThrowIfNull(originalList);

        if((additionalElements is null) || (additionalElements.Count < 1))
        {
            return originalList;
        }

        foreach (string additionalKeyword in additionalElements)
        {
            if (!originalList.Contains(additionalKeyword))
            {
                originalList.Add(additionalKeyword);
            }
        }

        return originalList;
    }


    public static object AppendValuesOfNewKeyValuePairsOnly(
        this List<string> originalList, Dictionary<string, string> additionalElements)
    {
        ArgumentNullException.ThrowIfNull(originalList);

        if ((additionalElements is null) || (additionalElements.Count < 1))
        {
            return originalList;
        }

        foreach (var keyValuePair in additionalElements)
        {
            if ((!originalList.Contains(keyValuePair.Key)) && (!originalList.Contains(keyValuePair.Value)))
            {
                originalList.Add(keyValuePair.Value);
            }
        }

        return originalList;
    }


    public static bool HasElement(this List<string> list, string text, 
        StringComparison stringComparison = StringComparison.Ordinal)
    {
        if(list.IsNullOrEmpty() || string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return list.Any(listElement => listElement.Equals(text, stringComparison));
    }


    public static bool IsMemberOf(this string text, List<string> list, 
        StringComparison stringComparison = StringComparison.Ordinal) => 
        list.HasElement(text, stringComparison);
}

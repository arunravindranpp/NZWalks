using ConflictAutomation.Extensions;
using ConflictAutomation.Models.PreScreening.SubClasses;

namespace ConflictAutomation.Mappers;

public static class TriggerForCheckMapper
{
    private static TriggerForCheck CreateFrom(KeyValuePair<string, string> keyValuePair) => new()
    {
        TriggerType = keyValuePair.Key,
        Details = keyValuePair.Value
    };


    public static List<TriggerForCheck> CreateFrom(Dictionary<string, string> dictionary) =>
        dictionary.IsNullOrEmpty() ? [] : dictionary.Select(CreateFrom).ToList();
}

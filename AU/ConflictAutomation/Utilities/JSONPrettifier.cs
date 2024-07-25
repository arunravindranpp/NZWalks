using System.Text.Json;

namespace ConflictAutomation.Utilities;

public static class JSONPrettifier
{
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

    public static string Format(string unPrettyJson)
    {
        // Source: https://stackoverflow.com/questions/4580397/json-formatter-in-c
        //         See response posted on 2020-08-24

        var jsonElement = JsonSerializer.Deserialize<JsonElement>(unPrettyJson);

        return JsonSerializer.Serialize(jsonElement, options);
    }


    public static string Format(object obj) => (obj is null)? "null" : Format(JsonSerializer.Serialize(obj));
}

using ConflictAutomation.Models;

namespace ConflictAutomation.Extensions;

public static class OnshoreEntityExtensions
{
    public static string GetPrimaryContact(this List<OnshoreEntity> listOnshoreData, 
        string countryName, string serviceLine, string subServiceLineCode)
    {
        string result = string.Empty;

        if (listOnshoreData.IsNullOrEmpty())
        {
            return result;
        }

        OnshoreEntity onshoreEntity = null;

        // 1st try: Search by country and serviceLine
        string serviceLineName = GetServiceLineName(serviceLine);
        onshoreEntity = listOnshoreData.FirstOrDefault(onshoreDataRow =>
                            onshoreDataRow.Country.Contains(countryName, StringComparison.OrdinalIgnoreCase)
                            && onshoreDataRow.ServiceLine.Contains(serviceLineName, StringComparison.OrdinalIgnoreCase));

        // 2nd try: Search by country and the Service Line computed from the subServiceLineCode prefix
        if (onshoreEntity is null)
        {
            var subServiceLineCodePrefix = subServiceLineCode.Trim().Left(2);
            if (_serviceLineCodePrefixToName.TryGetValue(subServiceLineCodePrefix, out serviceLineName))
            {
                onshoreEntity = listOnshoreData.FirstOrDefault(onshoreDataRow =>
                                    onshoreDataRow.Country.Contains(countryName, StringComparison.OrdinalIgnoreCase)
                                    && onshoreDataRow.ServiceLine.Contains(serviceLineName, StringComparison.OrdinalIgnoreCase));
            }
        }

        // 3rd try: Search by country and the Service Line = "All"
        if (onshoreEntity is null)
        {
            onshoreEntity ??= listOnshoreData.FirstOrDefault(onshoreDataRow =>
                                onshoreDataRow.Country.Contains(countryName, StringComparison.OrdinalIgnoreCase)
                                && onshoreDataRow.ServiceLine.Contains("All", StringComparison.OrdinalIgnoreCase));
        }

        result = onshoreEntity?.PrimaryContact ?? string.Empty;
        return result;
    }


    private static string GetServiceLineName(string serviceLine)
    {
        if(string.IsNullOrWhiteSpace(serviceLine))
        {
            return string.Empty;
        }

        var result = serviceLine.FullTrim(); 
        result = $"{result}(".StrLeft("(").FullTrim();
        result = $"{result}/".StrLeft("/").FullTrim();
        result = result.Replace("Strategy and Transactions", "SaT", StringComparison.OrdinalIgnoreCase);
        result = result.Replace("Strategy and Transaction",  "SaT", StringComparison.OrdinalIgnoreCase);
        return result;
    }


    private static readonly Dictionary<string, string> _serviceLineCodePrefixToName = new()
    {
        { "01", "Assurance" }, { "10", "Assurance" }, 
        { "02", "Tax" }, { "20", "Tax" },
        { "03", "Consulting" }, { "30", "Consulting" },
        { "07", "SaT" }, { "70", "SaT" }
    };
}


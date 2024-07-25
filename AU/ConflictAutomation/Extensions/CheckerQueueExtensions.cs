using ConflictAutomation.Constants;
using ConflictAutomation.Models;

namespace ConflictAutomation.Extensions;

public static class CheckerQueueExtensions
{
    public static bool IsPursuit(this CheckerQueue summary) =>
        summary.ConflictCheckType.Equals(CAUConstants.CONFLICT_CHECK_TYPE_PURSUIT);


    public static bool IsPursuitException(this CheckerQueue summary) => IsUSA(summary) || IsOceania(summary);


    private static bool IsUSA(CheckerQueue summary)
    {
        if ((summary is null) || (summary.CountryName is null))
        {
            return false;
        }

        string countryName = summary.CountryName.FullTrim();

        return countryName.Equals(CAUConstants.GEO_COUNTRY_NAME_US.FullTrim(), StringComparison.OrdinalIgnoreCase)
            || countryName.Equals(CAUConstants.GEO_COUNTRY_NAME_USA.FullTrim(), StringComparison.OrdinalIgnoreCase)
            || countryName.Equals(CAUConstants.GEO_COUNTRY_NAME_UNITED_STATES.FullTrim(), StringComparison.OrdinalIgnoreCase)
            || countryName.Equals(CAUConstants.GEO_COUNTRY_NAME_UNITED_STATES_OF_AMERICA.FullTrim(), StringComparison.OrdinalIgnoreCase);
    }


    private static bool IsOceania(CheckerQueue summary)
    {
        if ((summary is null) || (summary.Region is null))
        {
            return false;
        }

        string region = summary.Region.FullTrim();

        return region.Equals(CAUConstants.GEO_REGION_OCEANIA.FullTrim(), StringComparison.OrdinalIgnoreCase);
    }
}

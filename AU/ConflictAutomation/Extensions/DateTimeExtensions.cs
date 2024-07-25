namespace ConflictAutomation.Extensions;

public static class DateTimeExtensions
{
    public const string IST_TIMEZONE = "India Standard Time";
    public const string IST = "IST";
    public const string USER_FRIENDLY_TIMESTAMP_MASK = "{###} MMM, yyyy HH:mm:ss";  // In this class, {###} is replaced by the 
                                                                                    // day ordinal; e.g.: 1st, 3rd, 25th, etc.
    public const string USER_FRIENDLY_SHORT_TIMESTAMP_MASK = "yyyyMMddHHmm";


    public static DateTime FromLocalToTimezone(this DateTime localDateTime, string targetTimezone)
    {
        TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(targetTimezone);
        DateTime result = TimeZoneInfo.ConvertTime(localDateTime, TimeZoneInfo.Local, targetTimeZone);
        return result;
    }


    public static DateTime FromUtcToTimezone(this DateTime utcDateTime, string targetTimezone)
    {
        TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(targetTimezone);
        DateTime result = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, targetTimeZone);
        return result;
    }


    public static string TimestampWithTimezoneFromLocal(
        this DateTime localDateTime,
        string targetTimezone = IST_TIMEZONE, string targetTimezoneAbbreviation = IST,
        string dateTimeMask = USER_FRIENDLY_TIMESTAMP_MASK)
    {
        DateTime equivalentDateTime = localDateTime.FromLocalToTimezone(targetTimezone);
        string result = $"{equivalentDateTime.ToString(dateTimeMask)} {targetTimezoneAbbreviation}";
        result = result.ApplyDayOrdinal(equivalentDateTime);
        return result.FullTrim();
    }


    public static string TimestampWithTimezoneFromUtc(
        this DateTime utcDateTime,
        string targetTimezone = IST_TIMEZONE, string targetTimezoneAbbreviation = IST,
        string dateTimeMask = USER_FRIENDLY_TIMESTAMP_MASK)
    {
        DateTime equivalentDateTime = utcDateTime.FromUtcToTimezone(targetTimezone);
        string result = $"{equivalentDateTime.ToString(dateTimeMask)} {targetTimezoneAbbreviation}";
        result = result.ApplyDayOrdinal(equivalentDateTime);
        return result.FullTrim();
    }


    public static string TimestampWithoutTimezone(
        this DateTime dateTime,
        string dateTimeMask = USER_FRIENDLY_TIMESTAMP_MASK)
    {
        string result = $"{dateTime.ToString(dateTimeMask)}";
        result = result.ApplyDayOrdinal(dateTime);
        return result.FullTrim();
    }


    public static string ShortTimestampFromUtc(this DateTime utcDateTime,
                                               string targetTimezone = IST_TIMEZONE) =>
        utcDateTime.TimestampWithTimezoneFromUtc(  // Never mind the "...WithTimezone..." in this call,
                                                   // because parameter targetTimezoneAbbreviation
                                                   // will receive an empty string. The utcDateTime will
                                                   // be converted to the target timezone (IST by default), 
                                                   // but a timezone abbreviation will not be displayed.
            targetTimezone, targetTimezoneAbbreviation: string.Empty,
            dateTimeMask: USER_FRIENDLY_SHORT_TIMESTAMP_MASK);


    public static string DayOrdinal(this DateTime dateTime)
    {
        int day = dateTime.Day;
        var s = day.ToString();

        day %= 100;
        if ((11 <= day) && (day <= 13))
        {
            return $"{s}th";
        }

        return (day % 10) switch { 1 => $"{s}st", 2 => $"{s}nd", 3 => $"{s}rd", _ => $"{s}th" };
    }


    public static int MonthAbbrev3ToNumber(this string monthAbbrev3) =>
        monthAbbrev3.FullTrim().ToUpper() switch
        {
            "JAN" => 1,
            "FEB" => 2,
            "MAR" => 3,
            "APR" => 4,
            "MAY" => 5,
            "JUN" => 6,
            "JUL" => 7,
            "AUG" => 8,
            "SEP" => 9,
            "OCT" => 10,
            "NOV" => 11,
            "DEC" => 12,
            _ => 0,
        };


    private static string ApplyDayOrdinal(this string text, DateTime dateTime) =>
        text.Replace("{###}", dateTime.DayOrdinal());
}

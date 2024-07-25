using System.Data.SqlClient;

namespace ConflictAutomation.Extensions;

internal static class SqlReaderExtensions
{
    public static Dictionary<string, string> ToDictionaryStringString(this SqlDataReader sqlDataReader) =>
        sqlDataReader.ToDictionaryStringString("Key", "Value", s => s.UnencodeUnicodeChars());


    public static Dictionary<string, string> ToDictionaryStringString(this SqlDataReader sqlDataReader, string keyColumn, string valueColumn, Func<string, string> stringConvertion)
    {
        Dictionary<string, string> result = [];

        if (sqlDataReader is null)
        {
            return result;
        }

        while (sqlDataReader.Read())
        {
            var key = sqlDataReader[keyColumn].ToString();
            if (key is null)
            {
                continue;
            }
            key = stringConvertion(key);

            var value = sqlDataReader[valueColumn].ToString();
            if (value is null)
            {
                continue;
            }

            value = stringConvertion(value);

            result.Add(key, value);
        }

        return result;
    }


    public static Dictionary<string, string[]> ToDictionaryStringArrayOfStrings(this SqlDataReader sqlDataReader) =>
        sqlDataReader.ToDictionaryStringArrayOfStrings(
            "Key", ["Value1", "Value2", "Value3", "Value4"], s => s.UnencodeUnicodeChars());


    public static Dictionary<string, string[]> ToDictionaryStringArrayOfStrings(this SqlDataReader sqlDataReader, string keyColumn, string[] valueColumns, Func<string, string> stringConvertion)
    {
        Dictionary<string, string[]> result = [];

        if (sqlDataReader is null)
        {
            return result;
        }

        while (sqlDataReader.Read())
        {
            var key = sqlDataReader[keyColumn].ToString();
            if (key is null)
            {
                continue;
            }
            key = stringConvertion(key);

            List<string> values = [];
            foreach (var valueColumn in valueColumns)
            {
                string value = sqlDataReader[valueColumn].ToString();
                value ??= string.Empty;
                value = stringConvertion(value);

                values.Add(value);
            }

#pragma warning disable IDE0305 // Simplify collection initialization
            result.Add(key, values.ToArray());
#pragma warning restore IDE0305 // Simplify collection initialization
        }

        return result;
    }


    public static List<string> ToListString(this SqlDataReader sqlDataReader) =>
        sqlDataReader.ToListString("Key", s => s.UnencodeUnicodeChars());


    public static List<string> ToListString(this SqlDataReader sqlDataReader, string keyColumn, Func<string, string> stringConvertion)
    {
        List<string> result = [];

        if (sqlDataReader is null)
        {
            return result;
        }

        while (sqlDataReader.Read())
        {
            var key = sqlDataReader[keyColumn].ToString();
            if (key is null)
            {
                continue;
            }
            key = stringConvertion(key);

            result.Add(key);
        }

        return result;
    }
}

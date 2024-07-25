using ConflictAutomation.Extensions;
using System.Data;
using System.Data.SqlClient;

namespace ConflictAutomation.Utilities;

public class KeyValuePairs
{
    private readonly string _connectionString;
    private readonly bool _useCache;
    private readonly Dictionary<string, string> _keyValuePairs;

    public KeyValuePairs(string connectionString, bool useCache = true)
    {
        _connectionString = connectionString;
        _useCache = useCache;
        _keyValuePairs = _useCache ? LoadAllKeyValuePairs() : [];
    }


    private Dictionary<string, string> LoadAllKeyValuePairs()
    {
        string sql = "SELECT [Key], [Value] FROM [dbo].[CAU_KeyValuePairs]";
        SqlDataReader sqlDataReader = EYSql.ExecuteReader(_connectionString, CommandType.Text, sql);
        return sqlDataReader.HasRows ? sqlDataReader.ToDictionaryStringString() : [];
    }


    public string GetValue(string key, string defaultValue = null)
    {
        if(_useCache)
        {
            return _keyValuePairs[key];
        }

        string sql = "SELECT [Value] FROM [dbo].[CAU_KeyValuePairs] WHERE [Key] = @Key";
        SqlDataReader sqlDataReader = EYSql.ExecuteReader(_connectionString, CommandType.Text, sql, 
                                        new SqlParameter("@Key", key));        
        if (sqlDataReader.HasRows)
        {
            sqlDataReader.Read();            
            return sqlDataReader[0].ToString();
        }
        else
            return defaultValue;
    }


    public int GetValueAsInt(string key, int defaultValue = 0)
    {
        string valueStr = GetValue(key);
        if(string.IsNullOrEmpty(valueStr))
        {
            return defaultValue;
        }

        if (!int.TryParse(valueStr, out int result))
        {
            return defaultValue;
        }

        return result;
    }
}

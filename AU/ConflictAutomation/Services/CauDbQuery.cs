using ConflictAutomation.Extensions;
using System.Data;

namespace ConflictAutomation.Services;

public class CauDbQuery
{
    private const string SP_GET_GCO_TEAM_BY_COUNTRY_NAME = "dbo.SP_GET_GCO_TEAM_BY_COUNTRY_NAME";
    private const string SP_GET_RM_CONTACTS_BY_COUNTRY_NAME = "dbo.SP_GET_RM_CONTACTS_BY_COUNTRY_NAME";

    private const string COL_GCO_TEAM = "GCO_TEAM";
    private const string COL_RM_CONTACTS = "RM_CONTACTS";

    private const string SEP_SLASH = " / ";

    private readonly string _connectionString;


    public CauDbQuery(string connectionString)
    {
        if(string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        _connectionString = connectionString;
    }


    public string GetGcoTeamByCountryName(string countryName)
    {
        countryName ??= string.Empty;
        var sqlDataReader = EYSql.ExecuteReader(_connectionString, SP_GET_GCO_TEAM_BY_COUNTRY_NAME, [countryName]);
        List<string> gcoTeam = sqlDataReader
                                 .ToListString(COL_GCO_TEAM, x => FormatContacts(x))
                                 .Distinct(StringComparer.OrdinalIgnoreCase)
                                 .ToList();

        return string.Join(SEP_SLASH, gcoTeam).FullTrim();
    }


    public string GetRmContactsByCountryName(string countryName)
    {
        countryName ??= string.Empty; 
        var sqlDataReader = EYSql.ExecuteReader(_connectionString, SP_GET_RM_CONTACTS_BY_COUNTRY_NAME, [countryName]);
        List<string> rmContacts = sqlDataReader
                                     .ToListString(COL_RM_CONTACTS, x => FormatContacts(x))
                                     .Distinct(StringComparer.OrdinalIgnoreCase)
                                     .ToList();

        return string.Join(SEP_SLASH, rmContacts).FullTrim();
    }


    public string GetSubServiceLineCode(long conflictCheckId)
    {
        string result = string.Empty;

        var sql = $"SELECT TOP 1 SubServiceLineCode FROM WF_ConflictChecks WHERE ConflictCheckID = {conflictCheckId}";
        var sqlDataReader = EYSql.ExecuteReader(_connectionString, CommandType.Text, sql);
        if (sqlDataReader.HasRows)
        {
            result = sqlDataReader.ToListString("SubServiceLineCode", x => x.FullTrim()).First();
        }        

        return result;
    }


    private static string FormatContacts(string contacts) =>
        contacts.Contains('@') ? contacts.FullTrim() : contacts.FullTrim().ToUpper();
}

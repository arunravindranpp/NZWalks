using ConflictAutomation.Extensions;
using ConflictAutomation.Services.KeyGen.Enums;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace ConflictAutomation.Services.KeyGen;

public partial class KeyGenFactory
{
    const string SQL_DIACRITICS_REPLACEMENTS =
        "select FromValue as 'Key', ToValue_Step_1 as 'Value1', ToValue_Step_2 as 'Value2', ToValue_Step_3 as 'Value3', ToValue_Step_4 as 'Value4' from dbo.vwKeywordGeneratorDiacriticsReplacements";


    const string SQL_SUBSTRING_REPLACEMENTS_FOR_ENTITIES =
        "select FromValue as 'Key', ToValue as 'Value' from dbo.vwKeywordGeneratorForEntitiesSubstringReplacements";
    
    const string SQL_SPECIAL_CHARACTER_REPLACEMENTS_FOR_ENTITIES =
        "select FromValue as 'Key', ToValue as 'Value' from dbo.vwKeywordGeneratorForEntitiesSpecialCharacterReplacements";

    const string SQL_PREFIX_REMOVALS_FOR_ENTITIES =
        "select FromValue as 'Key' from dbo.vwKeywordGeneratorForEntitiesPrefixRemovals";

    const string SQL_PREFIX_GEO_LOCATION_NAMES_FOR_ENTITIES =
        "select FromValue as 'Key' from dbo.vwKeywordGeneratorForEntitiesGeoLocationNames";


    const string SQL_SUBSTRING_REPLACEMENTS_FOR_INDIVIDUALS =
        "select FromValue as 'Key', ToValue as 'Value' from dbo.vwKeywordGeneratorForIndividualsSubstringReplacements";

    const string SQL_SPECIAL_CHARACTER_REPLACEMENTS_FOR_INDIVIDUALS =
        "select FromValue as 'Key', ToValue as 'Value' from dbo.vwKeywordGeneratorForIndividualsSpecialCharacterReplacements";


    private readonly Dictionary<string, string> _substringReplacementsForEntities;
    private readonly Dictionary<string, string> _specialCharacterReplacementsForEntities;
    private readonly List<string> _prefixRemovalsForEntities;
    private readonly List<string> _geoLocationNamesForEntities;

    private readonly Dictionary<string, string> _substringReplacementsForIndividuals;
    private readonly Dictionary<string, string> _specialCharacterReplacementsForIndividuals;

    private readonly Dictionary<string, string[]> _diacriticsReplacements;


    public string ConnectionString { get; init; }

    public KeyGenForEntities KeyGenForEntities { get; init; }

    public KeyGenForIndividuals KeyGenForIndividuals { get; init; }


    public KeyGenFactory(string connectionString)
    {
        ConnectionString = connectionString;

        _substringReplacementsForEntities = GetDictionaryStringString(SQL_SUBSTRING_REPLACEMENTS_FOR_ENTITIES);
        _specialCharacterReplacementsForEntities = GetDictionaryStringString(SQL_SPECIAL_CHARACTER_REPLACEMENTS_FOR_ENTITIES);
        _prefixRemovalsForEntities = GetListString(SQL_PREFIX_REMOVALS_FOR_ENTITIES);
        _geoLocationNamesForEntities = GetListString(SQL_PREFIX_GEO_LOCATION_NAMES_FOR_ENTITIES);

        _substringReplacementsForIndividuals = GetDictionaryStringString(SQL_SUBSTRING_REPLACEMENTS_FOR_INDIVIDUALS);
        _specialCharacterReplacementsForIndividuals = GetDictionaryStringString(SQL_SPECIAL_CHARACTER_REPLACEMENTS_FOR_INDIVIDUALS);

        _diacriticsReplacements = GetDictionaryStringArrayOfStrings(SQL_DIACRITICS_REPLACEMENTS);

        KeyGenForEntities = new KeyGenForEntities(
                                    _substringReplacementsForEntities,
                                    _specialCharacterReplacementsForEntities,
                                    _diacriticsReplacements,
                                    _prefixRemovalsForEntities,
                                    _geoLocationNamesForEntities
                                );

        KeyGenForIndividuals = new KeyGenForIndividuals(
                                    _substringReplacementsForIndividuals, 
                                    _specialCharacterReplacementsForIndividuals,
                                    _diacriticsReplacements
                                );
    }


    public SubjectTypeEnum GetSubjectType(string name, string dunsNumber, string gisId, string paceApgLocation)
    {
        if (string.IsNullOrEmpty(name))
        {
            return SubjectTypeEnum.UnableToDecide;
        }
        else if (!string.IsNullOrWhiteSpace(dunsNumber))
        {
            return SubjectTypeEnum.Entity;
        }
        else if (KeyGen.SurroundSpecialCharactersWithSpaces(name)
                 .ContainsAnyOfSubstringReplacements(
                    _substringReplacementsForEntities.Where(r => r.Key.Length > 1).ToDictionary<string, string>())
                 )
        {
            return SubjectTypeEnum.Entity;
        }
        else if (name.ReplaceAll(_specialCharacterReplacementsForEntities).Replace(",", string.Empty).FullTrim()
                     .ContainsAnyOfSubstringReplacements(
                        _substringReplacementsForEntities.Where(r => r.Key.Length > 1).ToDictionary<string, string>()))
        {
            return SubjectTypeEnum.Entity;
        }
        else if ( (!string.IsNullOrWhiteSpace(gisId)) && string.IsNullOrWhiteSpace(paceApgLocation) )
        {
            return SubjectTypeEnum.Individual;
        }
        else if (name.ContainsAnyOfSubstringReplacements(
                        _substringReplacementsForIndividuals.Where(r => r.Key.Length > 1).ToDictionary<string, string>()))
        {
            return SubjectTypeEnum.Individual;
        }
        else if (name.ReplaceAll(_specialCharacterReplacementsForIndividuals).Replace(",", string.Empty).FullTrim()
                     .ContainsAnyOfSubstringReplacements(
                        _substringReplacementsForIndividuals.Where(r => r.Key.Length > 1).ToDictionary<string, string>()))
        {
            return SubjectTypeEnum.Individual;
        }
        else if (MatchesIndirectNamePattern(name
                                              .Replace("'", string.Empty)
                                              .Replace("’", string.Empty)
                                              .Replace("`", string.Empty)
                                              .Replace("´", string.Empty)
                                              .ConvertDiacriticsToStandardAnsi()))
        {
            return SubjectTypeEnum.Individual;
        }

        return SubjectTypeEnum.UnableToDecide;
    }


    private Dictionary<string, string> GetDictionaryStringString(string sql)
    {
        SqlDataReader sqlDataReader = GetSqlDataReader(sql);
        var result = sqlDataReader.ToDictionaryStringString();
        return result;
    }


    private List<string> GetListString(string sql)
    {
        SqlDataReader sqlDataReader = GetSqlDataReader(sql);
        var result = sqlDataReader.ToListString();
        return result;
    }


    private Dictionary<string, string[]> GetDictionaryStringArrayOfStrings(string sql)
    {
        SqlDataReader sqlDataReader = GetSqlDataReader(sql);        
        var result = sqlDataReader.ToDictionaryStringArrayOfStrings();
        return result;
    }


    private SqlDataReader GetSqlDataReader(string sql)
    {
        return EYSql.ExecuteReader(ConnectionString, CommandType.Text, sql);
    }


    private static bool MatchesIndirectNamePattern(string text)
    {
        Regex regex = IndirectNamePattern();
        return regex.IsMatch(text.FullTrim());
    }


    [GeneratedRegex(@"^[a-z\-'’`´\s]+\s*,[a-z\-'’`´\s]+(\s[a-z]\.\s*?)?$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex IndirectNamePattern();
}

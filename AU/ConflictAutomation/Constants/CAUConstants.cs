namespace ConflictAutomation.Constants;

public static class CAUConstants
{
    // Can be assigned to rs.ClientRelationshipSummary.GISNotes
    public const string GIS_MULTIPLE_CLOSE_MATCHES = "MULTIPLE CLOSE MATCHES IDENTIFIED IN GIS, PLEASE PERFORM FURTHER ANALYSIS";
    // Can be assigned to rs.ClientRelationshipSummary.CERNotes
    public const string CER_MULTIPLE_CLOSE_MATCHES = "MULTIPLE CLOSE MATCHES IDENTIFIED IN CER WITH BUSINESS NAME, PLEASE CONSIDER FURTHER ANALYSIS";

    // Can be assigned rs.ClientRelationshipSummary.GISDesc
    public const string GIS_ENTITY_UNDER_AUDIT_UNDEFINED__NO_RECORD_WITH_UID = "Entity Under Audit: (Yes / No) / No record found with UID (As per GIS)";
    public const string GIS_ENTITY_UNDER_AUDIT__NO_RECORD_WITH_UID =           "Entity Under Audit: Yes (As per GIS) / No record found with UID (As per GIS)";
    public const string GIS_ENTITY_NOT_UNDER_AUDIT__NO_RECORD_WITH_UID =       "Entity Under Audit: No (As per GIS) / No record found with UID (As per GIS)";
    public const string GIS_NO_RECORD_WITH_UID = "No record found with UID (As per GIS)";

    // Can be assigned rs.ClientRelationshipSummary.CERDesc
    public const string CER_NO_RECORD_WITH_UID_1 = "No record found with UID (As per CER)";
    public const string CER_NO_RECORD_WITH_UID_2 = "N/A (As per CER)";

    public const string NON_AUDIT_CLIENT = "Non-Audit Client";

    public const string MASTER_WORKBOOK_RESEARCH_UNIT_TEMPLATE_TAB = "(Entity Tmp)";
    public const string MASTER_WORKBOOK_SUMMARY_TAB = "Summary";
    public const string MASTER_WORKBOOK_PRESCREENING_INFO_TAB = "PreScreening Info";
    public const string MASTER_WORKBOOK_AU_UNIT_GRID_TAB = "AU Unit Grid";
    public const string MASTER_WORKBOOK_SUMMARY_PURSUIT_TAB = "Summary_Pursuit";
    public const string MASTER_WORKBOOK_PRESCREENING_INFO_PURSUIT_TAB = "PreScreening Info_Pursuit";

    public const string MASTER_WORKBOOK_SUMMARY_PREVIOUS = "Summary_Previous";
    public const string MASTER_WORKBOOK_PRESCREENING_INFO_PREVIOUS = "PreScreening Info_Previous";

    public const int ENTITY_TYPE_ID_CONFLICT_CHECK = 12;
    public const string ATTACHMENTS_FOR_ASSESSMENT_TEAM = "Conflicts-AttachmentsForAssessmentTeam";
    public const string ATTACHMENTS_FOR_CONSOLIDATED_RESEARCH_RESULTS = "Conflicts-AttachmentsForConsolidatedResearchResults";

    public const string CONFLICT_CHECK_TYPE_PURSUIT = "Pursuit";

    public const string GEO_COUNTRY_NAME_US = "US";
    public const string GEO_COUNTRY_NAME_USA = "USA";
    public const string GEO_COUNTRY_NAME_UNITED_STATES = "United States";
    public const string GEO_COUNTRY_NAME_UNITED_STATES_OF_AMERICA = "United States of America";
    public const string GEO_REGION_OCEANIA = "Oceania";

    public const string MSG_NO_DATA = "-";
    public const string MSG_INVALID_VALUE_FOR_CONCLUSION_SCENARIO = "Invalid value for ConclusionScenarioEnum";
    public const string MSG_TO_DO = "(to do)";
    public const string MSG_SECTION_GIS = "GIS";
    public const string MSG_SECTION_CER = "CER";
    public const string MSG_SECTION_CRR = "CRR";
    public const string MSG_SECTION_SPL = "SPL";
    public const string MSG_SECTION_FINSCAN = "FinScan";
    public const string MSG_SECTION_LOCAL_AND_EXTERNAL_DBS = "Local & External databases (including MDM)";

    public const string COLOR_ORANGE = "#FFA500";
    public const string COLOR_LIGHT_GRAY = "#BFBFBF";
    public const string COLOR_TEAL = "#B7DEE8";

    public const float STD_MIN_HEIGHT = 15.0f;
    public const float DEFAULT_FONT_SIZE = 10.0f;
    public const float SMALL_FONT_SIZE = 8.0f;
    public const float LARGE_FONT_SIZE = 12.0f;
    public const float TITLE_FONT_SIZE = 16.0f;
}

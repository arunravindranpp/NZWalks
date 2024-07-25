namespace ConflictAutomation.Services.FinScan;

public static class FinScanConstants
{
    public const string OUTPUT_REPORT_SUFFIX_AND_EXTENSION = "_fsProfile.xlsx";
    public const string MATCH_REPORT_SUFFIX_AND_EXTENSION = "_fsResults.xlsx";

    public const string STRING_SEPARATOR = "; ";

    // Keep the LISTID_ constants in UPPERCASE
    public const string LISTID_DJWL = "DJWL";
    public const string LISTID_DJSOC = "DJSOC";
    public const string LISTID_KH50 = "KH50";
    public const string LISTID_KHCO = "KHCO";

    public const string MSG_DJWL = "Dow Jones Watchlist";
    public const string MSG_DJSOC = "Dow Jones State Owned Companies List";
    public const string MSG_KH50 = "KH50";
    public const string MSG_KHCO = "KHCO";

    public const string SANCTIONED_COUNTRY_RUSSIA = "Russia";
    public const string SANCTIONED_COUNTRY_BELARUS = "Belarus";

    public const string Description2_SANCTIONS_LISTS = "Sanctions Lists";

    public const string ParentRelationship_SEPARATOR = "#::";
    public const string ParentRelationship_PARENT_COMPANY = "Parent Company";
    public const string ParentRelationship_EMPLOYER = "Employer";    

    #region Messages, labels and flags   
    public const string MSG_NA = "(n/a)";
    public const string MSG_INDIVIDUAL = "Individual";
    public const string MSG_ENTITY = "Entity";
    public const string MSG_FLAG_INDIVIDUAL = "I";
    public const string MSG_FLAG_ENTITY = "O";  // O = Organization(Entity)
    public const string MSG_ACTIVE = "Active";
    public const string MSG_INACTIVE = "Inactive";
    public const string MSG_YES = "Y";
    public const string MSG_NO = "N";
    public const string MSG_ORGANIZATION_NAME_SEARCH = "organization name search";
    public const string MSG_INDIVIDUAL_NAME_SEARCH = "individual name search";
    public const string MSG_FULL_NAME = "Full Name";
    public const string MSG_PARTIAL_NAME = "Partial Name";
    public const string MSG_SANCTIONED = "Sanctioned";
    public const string MSG_NOT_SANCTIONED = "Not Sanctioned";
    public const string MSG_APG = "APG";
    public const string MSG_NO_RESULTS_FOR = "No Results identified in FinScan with the keyword";
    public const string MSG_SEARCH = "search";
    public const string MSG_NOT_SANCTIONED_CHECK_MANUALLY = "AU identified the hit as not sanctioned. Please verify manually.";
    public const string MSG_REFER_TO_THE_ATTACHMENTS = "Refer to the attachment(s)";
    public const string MSG_PRIMARY = "Primary";

    public const string MSG_REPORT_TITLE = "List Profile Report";
    public const string MSG_NAMES_SEARCHED = "Names Searched";
    public const string MSG_REPORT_TYPE = "Report Type";
    public const string MSG_ENTITY_NAME = "Entity Name";
    public const string MSG_GENDER = "Gender";
    public const string MSG_RECORD_TYPE = "Record Type";
    public const string MSG_UID = "UID";
    public const string MSG_STATUS = "Status";
    public const string MSG_ORIGINAL_SCRIPT_NAME = "Original Script name";
    public const string MSG_LOAD_DATE = "Load Date";
    public const string MSG_VERSION = "Version";
    public const string MSG_DELETED = "Deleted";
    public const string MSG_ORIGINAL_TYPE = "Original Type";
    public const string MSG_COL_DATE_TYPE = "Date Type";
    public const string MSG_COL_DAY = "Day";
    public const string MSG_COL_MONTH = "Month";
    public const string MSG_COL_YEAR = "Year";
    public const string MSG_COL_DATE_NOTES = "Date Notes";
    public const string MSG_COL_DESCRIPTION_1 = "Description 1";
    public const string MSG_COL_DESCRIPTION_2 = "Description 2";
    public const string MSG_COL_DESCRIPTION_3 = "Description 3";
    public const string MSG_COL_SANCTION_DESCRIPTION_1 = "Sanction - Description 1";
    public const string MSG_COL_SANCTION_DESCRIPTION_2 = "Sanction - Description 2";
    public const string MSG_COL_SANCTION_NAME = "Sanction - Name";
    public const string MSG_COL_SINCE_DATE = "Since Date";
    public const string MSG_COL_STATUS = "Status";
    public const string MSG_COL_TO_DATE = "To Date";
    public const string MSG_COL_NAME_TYPE = "Name Type";
    public const string MSG_COL_TITLE_HONORIFIC = "Title Honorific";
    public const string MSG_COL_FIRST_NAME = "First Name";
    public const string MSG_COL_MIDDLE_NAME = "Middle Name";
    public const string MSG_COL_SURNAME = "Surname";
    public const string MSG_COL_SUFFIX = "Suffix";
    public const string MSG_COL_MAIDEN_NAME = "Maiden Name";
    public const string MSG_COL_ENTITY_NAME = "Entity Name";
    public const string MSG_COL_ORIGINAL_SCRIPT_NAME = "Original Script Name";
    public const string MSG_COL_SINGLE_STRING_NAME = "Single String Name";
    public const string MSG_COL_ADDRESS_CITY = "City";
    public const string MSG_COL_ADDRESS_COUNTRY = "Country";
    public const string MSG_COL_ADDRESS_LINE = "Address Line";
    public const string MSG_COL_ID_TYPE = "ID Type";
    public const string MSG_COL_VALUE = "Value";
    public const string MSG_COL_ID_NOTES = "ID Notes";
    public const string MSG_COL_COUNTRY_TYPE = "Country Type";
    public const string MSG_COL_COUNTRY = "Country";
    public const string MSG_COL_BIRTH_PLACE = "Birth Place";
    public const string MSG_COL_ASSOCIATE_NAME = "Associate Name";
    public const string MSG_COL_EX = "Ex";
    public const string MSG_COL_UID = "UID";
    public const string MSG_COL_RELATIONSHIP = "Relationship";
    public const string MSG_COL_TYPE = "Type";
    public const string MSG_COL_PEP = "PEP";
    public const string MSG_COL_PROFILE_NOTES = "Profile Notes";
    public const string MSG_COL_IMAGES = "Images";
    public const string MSG_COL_SOURCE_DESCRIPTION_NAME = "Source Description Name";
    public const string MSG_COL_ADDRESS_TYPE = "Address Type";
    public const string MSG_COL_ADDRESS_LINE_1 = "Address Line 1";
    public const string MSG_COL_CITY_LINE = "City Line";
    public const string MSG_COL_ENTITY = "Entity";
    public const string MSG_COL_PERSON = "Person";
    public const string MSG_COL_ID_NUMBER_TYPE = "ID Number Type";
    public const string MSG_COL_SUBTYPE = "Subtype";
    public const string MSG_COL_COUNTRY_ISSUED = "Country Issued";
    public const string MSG_COL_TEXT_INFO_ORIGINAL_TYPE = "Text Info - Original Type";
    public const string MSG_COL_TEXT_INFORMATION = "Text Information";
    public const string MSG_COL_TRACKING_INFORMATION = "Tracking Information";
    public const string MSG_COL_ORIGINAL_TYPE = "Original Type";
    public const string MSG_COL_URL_TYPE = "URL - Type";
    public const string MSG_COL_URL_VALUE = "URL - Value";
    public const string MSG_COL_URL_ORIGINAL_TYPE = "URL - Original Type";
    public const string MSG_SEARCH_API_ERROR = "Unable to search FinScan to generate results. Please investigate further.";
    public const string MSG_EY_CLIENTS_DISMISS_FINSCAN_SEARCH = "EY clients do not require FinScan research";  // User Story #1012887: Skip FinScan search for Main Clients
    public const string MSG_DOW_JONES_UNAVAILABLE = "(dowJones record unavailable for this match)";
    public const string MSG_INNOVATIVE_UNAVAILABLE = "(innovative record unavailable for this match)";
    public const string MSG_UNAVAILABLE = "(unavailable)";
    public const string MSG_LIST_NAME = "List Name";
    public const string MSG_LIST_CODE = "List Code";
    public const string MSG_ACTIVE_STATUS = "Active Status";
    public const string MSG_LIST_VERSION = "List Version";
    #endregion Messages, labels and flags
}

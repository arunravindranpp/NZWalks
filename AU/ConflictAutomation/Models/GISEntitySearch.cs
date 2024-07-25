namespace ConflictAutomation.Models;

#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0017 // Simplify object initialization
#pragma warning disable IDE0090 // Use 'new(...)'

public class EntitySearchCriteria
{
    public EntitySearchCriteria(string countryFilter, bool IsIndividual = false)
    {
        SearchExternal = false;
        StandardSearchParameters = new EntitySearchParameter(countryFilter, IsIndividual);
    }
    public bool SearchExternal { get; set; }
    public EntitySearchParameter StandardSearchParameters { get; set; }
}
public class EntitySearchParameter
{
    public EntitySearchParameter(string CountryFilter, bool IsIndividual = false)
    {
        DnBCountryCodeToSearch = "";
        ExcludeAlias = false;
        ExcludeGIS = false;
        IncludeDebug = true;
        IncludeGUPOnly = false;
        IncludeInactive = false;
        IncludeMDM = false;
        IsFuzzy = false;
        OnlyActive = true;
        OnlyInactive = false;
        OnlyOwned = false;
        SearchDnB = false;
        SearchProduction = false;
        SearchTerm = string.Empty;
        SearchwithinTree = false;
        UseBestFields = true;
        Filters = new List<Filter>();

        Filter defaultFilter = new Filter();
        defaultFilter.DefaultValue = IsIndividual ? "true" : "false"; //If true, Individual. If false, Entity
        defaultFilter.DisplayName = "Is Individual";
        defaultFilter.FilterType = "Text";
        defaultFilter.Id = "isIndividual";
        defaultFilter.Operator = "Is equal to";

        defaultFilter.Operators = new List<object>();
        defaultFilter.PlaceHolder = string.Empty;
        defaultFilter.Values = new List<object>();
        Filters.Add(defaultFilter);
        if (CountryFilter != string.Empty)
        {
            Filter countryFilter = new Filter();
            countryFilter.DefaultValue = CountryFilter;
            countryFilter.DisplayName = "Country";
            countryFilter.Error = string.Empty;
            countryFilter.FilterType = "Country";
            countryFilter.Id = "countryName";
            countryFilter.Operator = "In";
            countryFilter.Object = new List<string>();
            countryFilter.Object.Add(CountryFilter);
            countryFilter.Operators = new List<object>();
            countryFilter.PlaceHolder = string.Empty;
            countryFilter.PlaceHolder2 = "You may select more than one country";
            countryFilter.Values = new List<object>();
            Filters.Add(countryFilter);
        }
    }
    public string DnBCountryCodeToSearch { get; set; }
    public bool ExcludeAlias { get; set; }
    public bool ExcludeGIS { get; set; }

    public List<Filter> Filters { get; set; }
    public bool IncludeDebug { get; set; }
    public bool IncludeGUPOnly { get; set; }
    public bool IncludeInactive { get; set; }
    public bool IncludeMDM { get; set; }
    public bool IsFuzzy { get; set; }
    public bool OnlyActive { get; set; }
    public bool OnlyInactive { get; set; }
    public bool OnlyOwned { get; set; }
    public bool SearchDnB { get; set; }
    public bool SearchProduction { get; set; }
    public string SearchTerm { get; set; }
    public bool SearchwithinTree { get; set; }
    public bool UseBestFields { get; set; }
}
public class Filter
{
    public string Id { get; set; }
    public List<object> Operators { get; set; }
    public string DisplayName { get; set; }
    public string FilterType { get; set; }
    public List<object> Values { get; set; }
    public string DefaultValue { get; set; }
    public string Operator { get; set; }
    public string PlaceHolder { get; set; }
    public string PlaceHolder2 { get; set; }
    public List<string> Object { get; set; }
    public string Error { get; set; }
}

public class SearchEntitiesResponseViewModel
{
    public List<SearchEntitiesResponseItemViewModel> Records { get; set; }
    public int TotalRecords { get; set; }
}
public class GISEmbeddFiles
{
    public int EntityMatchCount;
    public List<SearchEntitiesResponseItemViewModel_SpreadSheet> EntityMatchRecords;
}
public class SearchEntitiesRequestViewModel
{
    public string SearchTerm { get; set; }
    public string Country { get; set; } = "";
}

public class SearchEntitiesResponseItemViewModel
{
    public long GisId { get; set; }
    public string EntityName { get; set; } //Business Name
    public string GUPName { get; set; } //Ultimate Parent
    public string GupDUNSNumber { get; set; }
    public string City { get; set; }//Street
    public string CountryName { get; set; }
    public string CountryCode { get; set; }
    public string StateProvinceCode { get; set; }
    public string StateProvinceName { get; set; }
    public string Gisp { get; set; }
    public string GupGisId { get; set; } //GUP ESD ID
    public string DUNSNumber { get; set; }
    public long MDMID { get; set; }
    public string GFISClientID { get; set; }
    public string NationalID { get; set; }
    public List<string> Aliases { get; set; }
    public Gcsp GCSP { get; set; }
    public List<AdditionalGcsps> AdditionalGcsps { get; set; }
    public Lap LAP { get; set; }
    public List<AdditionalLaps> AdditionalLaps { get; set; }
    //Restrictions Start- 
    public short BusinessRelationshipRestrictions { get; set; }
    public short AcpaRequired { get; set; }
    public short GcspApprovalRequired { get; set; }
    public bool Conflict { get; set; }
    public bool ConsultationRequired { get; set; }
    public short LoanRestrictions { get; set; }
    public bool ReverseRestriction { get; set; }


    public bool IsAuditClient { get; set; }
    public bool IsAttestAup { get; set; }
    public bool IsSustainability { get; set; }
    public short ScopeOfService { get; set; }
    //Restrictions Ends.

    public bool ParentRelationships { get; set; }
    public bool Subsidiaries { get; set; }
    public bool HasIndirectParents { get; set; }
}
public class SearchEntitiesResponseItemViewModel_SpreadSheet
{
    public long GISID { get; set; }
    public string UltimateParent { get; set; }
    public int BN_Fuzzy { get; set; }   //Business Name Fuzzy % - computed column
    public int AN_Fuzzy { get; set; }   //Alias Name Fuzzy % - computed column
    public string IdentificationNumber { get; set; }
    public string BusinessName { get; set; }
    public string Alias { get; set; }
    public string GUPESDID { get; set; }
    public string UltimateBusinessName { get; set; }
    public string StateProvinceName { get; set; }
    public string City { get; set; }
    public string MarkupCodes { get; set; }
    public string GISP { get; set; }
    public string Country { get; set; }
    public string DUNS { get; set; }
    public string MDMID { get; set; }
    public string CountryCode { get;set; }

}


public class SearchEntitiesResponseItemViewModel_SpreadSheet_Source //This fields are in the second sheet. Needs to evaluate what all are needeed
{
    public string GUP { get; set; } //Y or N. FTV process. Anoop: GISID == GUP ? Y : N
    public int GISID { get; set; }
    public string DUNS { get; set; } //DUNSNumber
    public string GFISID { get; set; } //GFISClientID
    public long Mercury_MDMID { get; set; } //MDMID
    public string Registration { get; set; } //Athul doubts its not valid. 
    public string NationalID { get; set; }
    public string TaxID { get; set; }//TAXIDENT
    public string CIK { get; set; } //CIK
    public long? PermId { get; set; } //PermId
    public string LegalEntityName { get; set; }//BusinessName
    public string AliasName { get; set; }//done already for other case. List<Aliases>
    public string GUPID { get; set; } //GupGisId
    public string GlobalUltimateParentName { get; set; } //GUPName
    public string Country { get; set; } //CountryName
    public string Restrictions { get; set; }//MarkupCodes
    //public string GCSP { get; set; } //Gcsp.FirstName + Gcsp.LastName
    public Gcsp GCSP { get; set; }
    public string GCSPGPN { get; set; } //Gcsp.Gpn
    public string LAP { get; set; } //Lap.FirstName + Lap.LastName
    public string LAPGPN { get; set; } //Lap.Gpn
    public string ActiveEntity { get; set; } //Athul doubts its not valid.
    public string Address1 { get; set; } //Address1
    public string Address2 { get; set; } //Address2
    public string Address3 { get; set; } //Address3
    public string City { get; set; } //City
    public string State { get; set; } //StateProvinceName
    public string MDMStage { get; set; } //Athul doubts its not needed.
    public string AdditionalGCSPGPN1 { get; set; } //Athul beleived remaining are not needed upto AdditionalLAPGPN4. Check. Structure needed. Anoop says its same structure as of Gcsp
    public string AdditionalGCSP2 { get; set; } //Check. Structure needed.
    public string AdditionalGCSPGPN2 { get; set; } //Check. Structure needed.
    public string AdditionalGCSP3 { get; set; }//Check. Structure needed.
    public string AdditionalGCSPGPN3 { get; set; }//Check. Structure needed.
    public string AdditionalGCSP4 { get; set; }//Check. Structure needed.
    public string AdditionalGCSPGPN4 { get; set; }//Check. Structure needed.
    public string AdditionalLAP1 { get; set; } //Check. Structure needed.
    public string AdditionalLAPGPN1 { get; set; } //Check. Structure needed.
    public string AdditionalLAP2 { get; set; } //Check. Structure needed.
    public string AdditionalLAPGPN2 { get; set; } //Check. Structure needed.
    public string AdditionalLAP3 { get; set; } //Check. Structure needed.
    public string AdditionalLAPGPN3 { get; set; } //Check. Structure needed.
    public string AdditionalLAP4 { get; set; } //Check. Structure needed.
    public string AdditionalLAPGPN4 { get; set; } //Check. Structure needed.

}


public class Gcsp
{
    public string Gui { get; set; }
    public string Gpn { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}
public class AdditionalGcsps
{
    public string Gui { get; set; }
    public string Gpn { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}

public class Lap
{
    public string Gui { get; set; }
    public string Gpn { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}

public class AdditionalLaps
{
    public string Gui { get; set; }
    public string Gpn { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}
public class UnitGrid_GISEntity
{
    public string KeywordUsed;
    public string EntityName_Results;
    public string AliasName;
    public string Country;
    public string DUNSNumber;
    public string Restrictions;
    public string EntityUnderAudit;
    public long GISID;
    public string GlobalUltimateParentName;
    public string GlobalUltimateParentCountry;//Anoop informed that this is presently not available. We might need to modify GIS web service
    public string GlobalUltimateParentDUNS;
    public string GCSP { get; set; } //Gcsp.FirstName + Gcsp.LastName
    public string LAP { get; set; } //Lap.FirstName + Lap.LastName
    public bool PIE { get; set; } //Anoop informed that this is presently not available. We might need to modify GIS web service
    public bool PIEAffiliate { get; set; } //Anoop informed that this is presently not available. We might need to modify GIS web service
    public bool Subsidiaries { get; set; }
    public bool G360 { get; set; }
    public string MDMGFISID { get; set; }
    public string PIEString { get; set; }
    public string PIEAffiliateString { get; set; }
    public string Notes { get; set; }
}


public class UnitGrid_MercuryEntity
{
    public string KeywordUsed;
    public string ClientName;
    public string DUNSName;
    public string DUNSLocation;
    public string DUNSNumber;
    public string GCSPName;
    public string EngagementPartnerName;
    public string UltimateParentName;
    public string UltimateParentDUNS;
}

public class UnitGrid_FinscanEntity
{
    public string KeywordUsed { get; init; }
    public string SearchType { get; init; }
    public int ActualMatches { get; init; }
    public string Relationship { get; init; }
    public string Sanction { get; init; }
    public string Source { get; init; }
    public string Comments { get; init; }
    public string CommentsInDetailsSection { get; init; }
    public string ListProfileReportFilePath { get; set; }
    public string MatchReportFilePath { get; set; }
    public string TargetTabName { get; init; }
}
public class ParentalRelationship
{
    public string BaseGISID { get; set; }
    public long GISID { get; set; }
    public string Entity { get; set; }
    public string RelationshipName { get; set; }
    public string SecondaryRelationshipAttribute { get; set; }
    public long ParentGISID { get; set; }
    public string ParentMDMID { get; set; }
    public string ParentDUNSNumber { get; set; }
    public string Country { get; set; }
    public string Type { get; set; }
    public string EntityWithoutLegalExt { get; set; }

}
public class GISSearch
{
    public string Enity { get; set; }
    public string DUNSNumber { get; set; }
    public string MDMID { get; set; }
    public string GISID { get; set; }
    public bool IsIndividual { get; set; }
}

public class FinalGISResults
{
    public string DUNSNumber;
    public string Restrictions;
    public bool EntityUnderAudit;
    public long GISID;
    public string GCSPName; //Gcsp.FirstName + Gcsp.LastName
    public string LAPName; //Lap.FirstName + Lap.LastName

    public string PIEString;
    public string PIEAffiliateString; // { get; set; }
    public string Notes; // { get; set; }
}

public class Restrictions
{
    public long GISID;
    public string RestrictionName;
}

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0017 // Simplify object initialization
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0290 // Use primary constructor

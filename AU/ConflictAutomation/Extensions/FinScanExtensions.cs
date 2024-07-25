using ConflictAutomation.Models.FinScan;
using ConflictAutomation.Models.FinScan.SubClasses;
using ConflictAutomation.Models.FinScan.SubClasses.enums;
using ConflictAutomation.Services.FinScan;

namespace ConflictAutomation.Extensions;

public static class FinScanExtensions
{
    public static bool NoResult(this FinScanResponse finScanResponse) =>
        (finScanResponse is null) || (finScanResponse.searchResults is null) || (finScanResponse.searchResults.Count < 1);


    public static int SearchMatchesCount(this FinScanResponse finScanResponse) =>
        finScanResponse.NoResult() ? 0 : finScanResponse.searchResults.First().searchMatches.Count;


    public static bool NoMatch(this FinScanResponse finScanResponse) =>
        finScanResponse.SearchMatchesCount() < 1;


    public static bool HasPossibleMatches(this FinScanResponse finScanResponse) => !finScanResponse.NoMatch();


    public static SearchTypeEnum IndividualOrEntity(this FinScanRequest finScanRequest)
    {
        SearchTypeEnum result = SearchTypeEnum.Default;  // Default means, in fact, unknown.

        if (finScanRequest is null)
        {
            return result;
        }

        switch (finScanRequest.searchType)
        {
            case SearchTypeEnum.Organization:
            case SearchTypeEnum.Individual:
                result = finScanRequest.searchType;
                break;

            default:
                break;
        }

        return result;
    }


    public static SearchTypeEnum IndividualOrEntity(this FinScanResponse finScanResponse)
    {
        SearchTypeEnum result = SearchTypeEnum.Default;  // Default means, in fact, unknown.

        if (finScanResponse is null)
        {
            return result;
        }

        switch (finScanResponse.searchResults.FirstOrDefault().searchName.FullTrim().ToLower())
        {
            case FinScanConstants.MSG_ORGANIZATION_NAME_SEARCH:
                result = SearchTypeEnum.Organization;
                break;


            case FinScanConstants.MSG_INDIVIDUAL_NAME_SEARCH:
                result = SearchTypeEnum.Individual;
                break;
        }

        return result;
    }


    public static DowJonesListRecord GetDowJonesListRecord(this SearchMatch searchMatch)
    {
        if (searchMatch is null)
        {
            return null;
        }        

        return searchMatch.listRecordDetail.listRecord.dowJones;
    }


    public static InnovativeListRecord GetInnovativeListRecord(this SearchMatch searchMatch)
    {
        if (searchMatch is null)
        {
            return null;
        }

        return searchMatch.listRecordDetail.listRecord.innovative;
    }


    public static DowJonesEntityOrPerson GetDowJonesEntityOrPerson(
        this FinScanRequest finScanRequest, DowJonesListRecord dowJonesListRecord)
    {
        DowJonesEntityOrPerson result = null;

        if ((finScanRequest is null) || (dowJonesListRecord is null))
        {
            return result;
        }

        switch (finScanRequest.IndividualOrEntity())
        {
            case SearchTypeEnum.Individual:
                result = dowJonesListRecord?.Person;
                break;

            case SearchTypeEnum.Organization:
                result = dowJonesListRecord?.Entity;
                break;
        }

        return result;
    }


    public static DowJonesEntityOrPerson GetDowJonesEntityOrPerson(
        this FinScanResponse finScanResponse, DowJonesListRecord dowJonesListRecord)
    {
        DowJonesEntityOrPerson result = null;

        if ((finScanResponse is null) || (dowJonesListRecord is null))
        {
            return result;
        }

        switch (finScanResponse.IndividualOrEntity())
        {
            case SearchTypeEnum.Individual:
                result = dowJonesListRecord?.Person;
                break;

            case SearchTypeEnum.Organization:
                result = dowJonesListRecord?.Entity;
                break;
        }

        return result;
    }


    public static InnovativeEntityOrPerson GetInnovativeEntityOrPerson(
        this FinScanResponse finScanResponse, InnovativeListRecord innovativeListRecord)
    {
        InnovativeEntityOrPerson result = null;

        if ((finScanResponse is null) || (innovativeListRecord is null))
        {
            return result;
        }

        switch (finScanResponse.IndividualOrEntity())
        {
            case SearchTypeEnum.Individual:
                result = innovativeListRecord.nameAddressRecord?.person.FirstOrDefault(p => 
                            p.type.Equals("Primary", StringComparison.OrdinalIgnoreCase));
                break;

            case SearchTypeEnum.Organization:
                result = innovativeListRecord.nameAddressRecord?.entity.FirstOrDefault(p =>
                            p.type.Equals("Primary", StringComparison.OrdinalIgnoreCase));
                break;
        }

        return result;
    }


    public static List<Address> GetAddressesColl(this FinScanResponse finScanResponse, DowJonesEntityOrPerson dowJonesEntityOrPerson)
    {
        List<Address> result;

#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (finScanResponse.IndividualOrEntity())
        {
            case SearchTypeEnum.Individual:
                result = (dowJonesEntityOrPerson as DowJonesPerson).Addresses is null ?
                                    [] : (dowJonesEntityOrPerson as DowJonesPerson).Addresses;
                break;

            case SearchTypeEnum.Organization:
                result = (dowJonesEntityOrPerson as DowJonesEntity).CompanyDetails is null ?
                                    [] : (dowJonesEntityOrPerson as DowJonesEntity).CompanyDetails;
                break;

            default:
                result = [];
                break;
        }
#pragma warning restore IDE0066 // Convert switch statement to expression

        return result;
    }


    public static FinScanListProfileReportItem MakeFinScanListProfileReportItem(
        this FinScanResponse finScanResponse, SearchMatch searchMatch) => 
        searchMatch.listId switch
        {
            FinScanConstants.LISTID_DJWL => finScanResponse.MakeFinScanListProfileReportItemForDJWL(searchMatch), 
            FinScanConstants.LISTID_DJSOC => finScanResponse.MakeFinScanListProfileReportItemForDJSOC(searchMatch), 
            FinScanConstants.LISTID_KH50 => finScanResponse.MakeFinScanListProfileReportItemForKharonFeed(searchMatch), 
            FinScanConstants.LISTID_KHCO => finScanResponse.MakeFinScanListProfileReportItemForKharonFeed(searchMatch), 
            _ => null
        };


    private static FinScanListProfileReportItem MakeFinScanListProfileReportItemForDJWL(
        this FinScanResponse finScanResponse, SearchMatch searchMatch)
    {
        if ((finScanResponse is null) || (searchMatch is null))
        {
            return null;
        }

        var dowJonesListRecord = searchMatch.GetDowJonesListRecord();
        var dowJonesEntityOrPerson = finScanResponse.GetDowJonesEntityOrPerson(dowJonesListRecord);

#pragma warning disable IDE0305 // Simplify collection initialization
        return new()
        {
            ListId = searchMatch.listId,

            Name = searchMatch.Name() + (((dowJonesListRecord is null) || (dowJonesEntityOrPerson is null)) 
                                            ? $" {FinScanConstants.MSG_DOW_JONES_UNAVAILABLE}" : string.Empty),
            Gender = searchMatch.Gender(),
            RecordType = RecordType(searchMatch),
            Uid = searchMatch.listProfileId,
            Status = (dowJonesEntityOrPerson is null) ? FinScanConstants.MSG_UNAVAILABLE : 
                        dowJonesEntityOrPerson.Status(),
            OriginalScriptName = (finScanResponse.IndividualOrEntity() == SearchTypeEnum.Individual) ? FinScanConstants.MSG_INDIVIDUAL :
                                    (finScanResponse.IndividualOrEntity() == SearchTypeEnum.Organization) ? FinScanConstants.MSG_ENTITY :
                                        string.Empty,
            LoadDate = (dowJonesListRecord is null) ? FinScanConstants.MSG_UNAVAILABLE : 
                            dowJonesListRecord.loadDate.TimestampWithoutTimezone(),
            Version = (dowJonesListRecord is null) ? FinScanConstants.MSG_UNAVAILABLE : 
                            dowJonesListRecord.listVersion,
            Deleted = (dowJonesListRecord is null) ? FinScanConstants.MSG_UNAVAILABLE :
                            dowJonesListRecord.deleted,
            DatesColl = (dowJonesEntityOrPerson is null) ? [] : 
                            dowJonesEntityOrPerson.DateDetails is null ? [] : dowJonesEntityOrPerson.DateDetails,
            DescriptionsColl = (dowJonesEntityOrPerson is null) ? [] : 
                                dowJonesEntityOrPerson.Descriptions is null ? [] : dowJonesEntityOrPerson.Descriptions,
            SanctionsReferencesColl = (dowJonesEntityOrPerson is null) ? [] : 
                                        dowJonesEntityOrPerson.SanctionsReferences is null ? [] : dowJonesEntityOrPerson.SanctionsReferences,
            NamesColl = (dowJonesEntityOrPerson is null) ? [] :
                            dowJonesEntityOrPerson.NameDetails is null ? [] : dowJonesEntityOrPerson.NameDetails,
            AddressesColl = (dowJonesEntityOrPerson is null) ? [] : 
                                finScanResponse.GetAddressesColl(dowJonesEntityOrPerson),
            IdsColl = (dowJonesEntityOrPerson is null) ? [] :
                        dowJonesEntityOrPerson.IDNumberTypes is null ? [] : dowJonesEntityOrPerson.IDNumberTypes,
            CountriesColl = (dowJonesEntityOrPerson is null) ? [] :
                                dowJonesEntityOrPerson.CountryDetails is null ? [] : dowJonesEntityOrPerson.CountryDetails,
            BirthPlaceColl = (dowJonesEntityOrPerson is null) ? [] : 
                                (finScanResponse.IndividualOrEntity() != SearchTypeEnum.Individual) ? null :
                                    (dowJonesEntityOrPerson as DowJonesPerson).BirthPlace is null ? [] :
                                        (dowJonesEntityOrPerson as DowJonesPerson).BirthPlace,
            AssociatesColl = (dowJonesListRecord is null) ? [] : 
                                dowJonesListRecord.Associations is null ? [] :
                                    dowJonesListRecord.Associations.OrderBy(associate => associate.singleStringName).ToList(),
            ProfileNotes = (dowJonesEntityOrPerson is null) ? string.Empty :
                                dowJonesEntityOrPerson.ProfileNotes is null ? string.Empty : dowJonesEntityOrPerson.ProfileNotes,
            ImagesColl = (dowJonesEntityOrPerson is null) ? [] : 
                            (finScanResponse.IndividualOrEntity() != SearchTypeEnum.Individual) ? null :
                                (dowJonesEntityOrPerson as DowJonesPerson).Images is null ? [] :
                                    (dowJonesEntityOrPerson as DowJonesPerson).Images,
            SourcesColl = (dowJonesEntityOrPerson is null) ? [] : 
                            dowJonesEntityOrPerson.SourceDescription is null ? [] : dowJonesEntityOrPerson.SourceDescription
        };
#pragma warning restore IDE0305 // Simplify collection initialization
    }


    private static FinScanListProfileReportItem MakeFinScanListProfileReportItemForDJSOC(
        this FinScanResponse finScanResponse, SearchMatch searchMatch) =>
            finScanResponse.MakeFinScanListProfileReportItemForDjsocOrKharonFeed(searchMatch,
                FinScanParser.SearchMatchIsSanctionedDJSOC);


    private static FinScanListProfileReportItem MakeFinScanListProfileReportItemForKharonFeed(
        this FinScanResponse finScanResponse, SearchMatch searchMatch) =>
            finScanResponse.MakeFinScanListProfileReportItemForDjsocOrKharonFeed(searchMatch,
                FinScanParser.SearchMatchIsSanctionedKharonFeed);


#pragma warning disable IDE0060 // Remove unused parameter
    private static FinScanListProfileReportItem MakeFinScanListProfileReportItemForDjsocOrKharonFeed(
        this FinScanResponse finScanResponse,SearchMatch searchMatch,
        Func<SearchMatch, bool?> isSanctionedFunction)
    {
        if ((finScanResponse is null) || (searchMatch is null))
        {
            return null;
        }

        var innovativeListRecord = searchMatch.GetInnovativeListRecord();

        return new()
        {
            ListId = searchMatch.listId,

            Name = searchMatch.Name() + 
                   ((innovativeListRecord is null) ? $" {FinScanConstants.MSG_INNOVATIVE_UNAVAILABLE}" : string.Empty),
            RecordType = RecordType(searchMatch),
            Status = (innovativeListRecord is null) || (innovativeListRecord.nameAddressRecord is null) 
                        ? FinScanConstants.MSG_UNAVAILABLE
                        : innovativeListRecord.nameAddressRecord.active == "Y" ? "Active" : "Inactive",
            Uid = searchMatch.listProfileId,
            LoadDate = (innovativeListRecord is null) ? FinScanConstants.MSG_UNAVAILABLE :
                            innovativeListRecord.loadDate.TimestampWithoutTimezone(),
            Version = (innovativeListRecord is null) ? FinScanConstants.MSG_UNAVAILABLE :
                            innovativeListRecord.listVersion,
            Deleted = (innovativeListRecord is null) ? FinScanConstants.MSG_UNAVAILABLE :
                            innovativeListRecord.deleted,
            InnovativeAddressesColl = (innovativeListRecord is null) || (innovativeListRecord.nameAddressRecord is null)
                                        || innovativeListRecord.nameAddressRecord.address.IsNullOrEmpty() ? []
                                        : innovativeListRecord.nameAddressRecord.address.Select(addr =>
                                            new InnovativeAddress()
                                            {
                                                AddressType = addr.type,
                                                AddressLine1 = addr.addressLines?.line1 ?? string.Empty,
                                                CityLine = addr.cityLine,
                                                Country = addr.country
                                            }
                                          ).ToList(),
            InnovativeDatesColl = (innovativeListRecord is null) || (innovativeListRecord.nameAddressRecord is null)
                        || (innovativeListRecord.nameAddressRecord.dates.IsNullOrEmpty())? []
                            : innovativeListRecord.nameAddressRecord.dates.Select(dt => 
                                new InnovativeDate() {
                                    Type = dt.type,
                                    Value = dt.value, 
                                    OriginalType = dt.additionalDateFields
                                                        .FirstOrDefault(field =>
                                                            (field.order == "1") &&
                                                            (field.type.Equals(
                                                                FinScanConstants.MSG_ORIGINAL_TYPE.FullTrim(),
                                                                StringComparison.OrdinalIgnoreCase)))?.value ?? string.Empty, 

                                }).ToList(),
            InnovativeEntitiesColl = (innovativeListRecord is null) || (innovativeListRecord.nameAddressRecord is null)
                                     || (innovativeListRecord.nameAddressRecord.entity.IsNullOrEmpty()) ? []
                                         : innovativeListRecord.nameAddressRecord.entity
                                            .Select(entity => entity.fullName)
                                            .ToList(),
            InnovativePersonsColl = (innovativeListRecord is null) || (innovativeListRecord.nameAddressRecord is null)
                         || (innovativeListRecord.nameAddressRecord.person.IsNullOrEmpty()) ? []
                             : innovativeListRecord.nameAddressRecord.person
                                .Select(person => person.fullName)
                                .ToList(),
            InnovativeIDsColl = (innovativeListRecord is null) || (innovativeListRecord.nameAddressRecord is null)
                                     || (innovativeListRecord.nameAddressRecord.idNumbers.IsNullOrEmpty()) ? []
                                         : innovativeListRecord.nameAddressRecord.idNumbers
                                            .Select(id => 
                                                new InnovativeID() 
                                                { 
                                                    IDNumberType = id.type, 
                                                    Subtype = id.subtype, 
                                                    Value = id.value, 
                                                    CountryIssued = id.countryIssued
                                                }
                                            ).ToList(),
            InnovativeTextInformationsColl = (innovativeListRecord is null) || (innovativeListRecord.nameAddressRecord is null)
                                             || (innovativeListRecord.nameAddressRecord.textInfo.IsNullOrEmpty()) ? []
                                                 : innovativeListRecord.nameAddressRecord.textInfo
                                                    .Select(textInfo => 
                                                        new InnovativeTextInformation() 
                                                        { 
                                                            OriginalType = textInfo.additionalFields.FirstOrDefault(field =>
                                                                            (field.order == "1") &&
                                                                            (field.type.Equals(FinScanConstants.MSG_ORIGINAL_TYPE.FullTrim(), 
                                                                                               StringComparison.OrdinalIgnoreCase)))?.value ?? string.Empty, 
                                                            TextInformation = textInfo.value 
                                                        }
                                                    ).ToList(),
            InnovativeTrackingInformationsColl = (innovativeListRecord is null) || (innovativeListRecord.nameAddressRecord is null)
                                                  || (innovativeListRecord.nameAddressRecord.trackingInformation.IsNullOrEmpty()) ? []
                                                      : innovativeListRecord.nameAddressRecord.trackingInformation
                                                         .Select(trackingInfo => 
                                                             new InnovativeTrackingInformation() 
                                                             { 
                                                                 Type = trackingInfo.type,
                                                                 Value = trackingInfo.value, 
                                                                 OriginalType = trackingInfo.additionalTrackingFields
                                                                                    .FirstOrDefault(field =>
                                                                                        (field.order == "1") &&
                                                                                        (field.type.Equals(
                                                                                            FinScanConstants.MSG_ORIGINAL_TYPE.FullTrim(),
                                                                                            StringComparison.OrdinalIgnoreCase)))?.value ?? string.Empty
                                                             }
                                                         ).ToList(),
            InnovativeURLsColl = (innovativeListRecord is null) || (innovativeListRecord.nameAddressRecord is null)
                                    || (innovativeListRecord.nameAddressRecord.trackingInformation.IsNullOrEmpty()) ? []
                                        : innovativeListRecord.nameAddressRecord.URLS
                                            .Select(url =>
                                                new InnovativeURL()
                                                {
                                                    Type = url.type,
                                                    Value = url.value,
                                                    OriginalType = url.additionalFields
                                                                    .FirstOrDefault(field =>
                                                                        (field.order == "1") &&
                                                                        (field.type.Equals(
                                                                            FinScanConstants.MSG_ORIGINAL_TYPE.FullTrim(),
                                                                            StringComparison.OrdinalIgnoreCase)))?.value ?? string.Empty
                                                }
                                            ).ToList()
        };
    }
#pragma warning restore IDE0060 // Remove unused parameter


    public static string Name(this SearchMatch searchMatch) =>
        (searchMatch.dynamicFields is null) ? searchMatch.displayLine :
            searchMatch.dynamicFields.listParentSingleStringName;


    public static string Gender(this SearchMatch searchMatch) =>
        IsIndividual(searchMatch.recordType) ? searchMatch.dynamicFields.listGender?.FullTrim() : FinScanConstants.MSG_NA;


    public static string RecordType(this SearchMatch searchMatch) =>
        IsIndividual(searchMatch.recordType) ? FinScanConstants.MSG_INDIVIDUAL :
            (IsEntity(searchMatch.recordType) ? FinScanConstants.MSG_ENTITY : string.Empty);


    private static bool IsIndividual(string recordType) =>
        recordType.Equals(FinScanConstants.MSG_FLAG_INDIVIDUAL, StringComparison.OrdinalIgnoreCase);


    private static bool IsEntity(string recordType) =>
        recordType.Equals(FinScanConstants.MSG_FLAG_ENTITY, StringComparison.OrdinalIgnoreCase);


    public static string Status(this DowJonesEntityOrPerson dowJonesEntityOrPerson) =>
        IsActive(dowJonesEntityOrPerson) ? FinScanConstants.MSG_ACTIVE :
            (IsInactive(dowJonesEntityOrPerson) ? FinScanConstants.MSG_INACTIVE : string.Empty);

    private static bool IsActive(DowJonesEntityOrPerson dowJonesEntityOrPerson) =>
        dowJonesEntityOrPerson.ActiveStatus.Equals(FinScanConstants.MSG_YES, StringComparison.OrdinalIgnoreCase);

    private static bool IsInactive(DowJonesEntityOrPerson dowJonesEntityOrPerson) =>
        dowJonesEntityOrPerson.ActiveStatus.Equals(FinScanConstants.MSG_NO, StringComparison.OrdinalIgnoreCase);


    public static string Status(this InnovativeListRecord innovativeListRecord) =>
        IsActive(innovativeListRecord) ? FinScanConstants.MSG_ACTIVE :
            (IsInactive(innovativeListRecord) ? FinScanConstants.MSG_INACTIVE : string.Empty);

    private static bool IsActive(InnovativeListRecord innovativeListRecord) =>
        innovativeListRecord.nameAddressRecord.active.Equals(FinScanConstants.MSG_YES, StringComparison.OrdinalIgnoreCase);

    private static bool IsInactive(InnovativeListRecord innovativeListRecord) =>
        innovativeListRecord.nameAddressRecord.active.Equals(FinScanConstants.MSG_NO, StringComparison.OrdinalIgnoreCase);
}
